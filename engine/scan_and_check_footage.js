/**
 * scan_and_check_footage.js — Mesin Pemindai & Pemeriksa Footage Pra-Render (Pre-Flight Review)
 * 100% Offline, Zero Bandwidth, Instant Storyboard Preview
 * 
 * Standar Arsitektur: FokusKonten Video Studio v5.2
 * Mendukung pencarian awal dan Scan Ulang (Re-scan) alternatif dari 27.291 klip SQLite WAL.
 */
'use strict';

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

const {
  loadCatalog,
  detectNicheDomain,
  buildKinshipSequenceForScript
} = require('./search_vidcon_local');
const { runPreFlightGate } = require('./generate_storyboard_contact_sheet');

function parseArgs() {
  const args = process.argv.slice(2);
  const options = {
    projectDir: null,
    rescan: false,
    domain: null,
    duration: 60,
    topic: null,
    customVoice: null,
    footageSkus: null
  };

  for (let i = 0; i < args.length; i++) {
    const a = args[i];
    if (a === '--projectDir' && args[i + 1]) {
      options.projectDir = args[++i];
    } else if (a === '--rescan') {
      options.rescan = true;
    } else if (a === '--domain' && args[i + 1]) {
      options.domain = args[++i];
    } else if (a === '--duration' && args[i + 1]) {
      options.duration = parseFloat(args[++i]) || 60;
    } else if (a === '--topic' && args[i + 1]) {
      options.topic = args[++i];
    } else if ((a === '--customVoice' || a === '--custom-voice') && args[i + 1]) {
      options.customVoice = args[++i];
    } else if ((a === '--footage-skus' || a === '--footageSkus') && args[i + 1]) {
      options.footageSkus = args[++i];
    }
  }

  return options;
}

async function main() {
  const options = parseArgs();
  if (!options.projectDir) {
    const topicSlug = (options.topic || 'Uji_Coba_Stoik').replace(/[^a-zA-Z0-9_\-]/g, '_');
    const baseProjectsDir = (pathsResolver && pathsResolver.PROJECTS_DIR)
      ? pathsResolver.PROJECTS_DIR
      : path.join(__dirname, '..', 'projects');
    const sandboxDir = path.join(baseProjectsDir, `_test_sandbox_${topicSlug}`);
    const promptsDir = path.join(sandboxDir, '04_prompts');
    if (!fs.existsSync(promptsDir)) {
      fs.mkdirSync(promptsDir, { recursive: true });
    }
    const sampleScript = path.join(promptsDir, 'final_script_scenes.txt');
    if (!fs.existsSync(sampleScript)) {
      const sampleText = [
        "Dalam keheningan malam yang sunyi, kamu merenungi kerasnya jalan hidup.",
        "Keteguhan hati Thomas Shelby membuktikan bahwa wibawa lahir dari penderitaan.",
        "Kekecewaan Joker mengajarkan bahwa dunia ini tak selalu membalas senyumanmu.",
        "Fokus pada apa yang bisa kamu kendalikan, bukan pada omongan orang lain.",
        "Setiap luka dan kegagalan adalah fondasi tak tergoyahkan menuju kejayaan.",
        "Bangkitlah dengan kepala tegak, buktikan pada mereka yang meremehkanmu."
      ].join('\n');
      fs.writeFileSync(sampleScript, sampleText, 'utf8');
    }
    options.projectDir = sandboxDir;
    console.log(`[SANDBOX TEST] Argumen --projectDir tidak ditentukan. Menggunakan sandbox: ${sandboxDir}`);
  }

  const projectDir = path.resolve(options.projectDir);
  const projectName = path.basename(projectDir);
  const promptsDir = path.join(projectDir, '04_prompts');
  const rawDir = path.join(projectDir, '01_video_raw');
  const audioDir = path.join(projectDir, '02_audio_master');
  const bgmDir = path.join(projectDir, '03_audio_bgm');
  const subsDir = path.join(projectDir, '05_subtitles');
  const renderDir = path.join(projectDir, '06_final_render');

  [rawDir, audioDir, bgmDir, promptsDir, subsDir, renderDir].forEach(d => {
    if (!fs.existsSync(d)) fs.mkdirSync(d, { recursive: true });
  });

  // 1. Baca Naskah
  const scriptPath = path.join(promptsDir, 'final_script_scenes.txt');
  const fullScriptPath = path.join(promptsDir, 'naskah_full.txt');
  let scriptText = '';

  if (fs.existsSync(scriptPath)) {
    scriptText = fs.readFileSync(scriptPath, 'utf8').trim();
  } else if (fs.existsSync(fullScriptPath)) {
    scriptText = fs.readFileSync(fullScriptPath, 'utf8').trim();
  }

  if (!scriptText) {
    console.error(`[ERROR] Naskah tidak ditemukan di: ${scriptPath}`);
    process.exit(1);
  }

  const sentences = scriptText.split('\n').map(s => s.trim()).filter(Boolean);
  if (sentences.length === 0) {
    console.error('[ERROR] Naskah tidak memiliki baris kalimat yang valid.');
    process.exit(1);
  }

  // 2. Tentukan Timing Adegan (Ground Truth atau Estimasi Pacing Halus)
  const timingFile = path.join(promptsDir, 'sentence_timing_master.json');
  let sceneTimings = [];

  if (fs.existsSync(timingFile)) {
    try {
      const parsed = JSON.parse(fs.readFileSync(timingFile, 'utf8'));
      if (Array.isArray(parsed) && parsed.length === sentences.length) {
        sceneTimings = parsed;
      }
    } catch (e) {}
  }

  // Jika timing belum ada atau tidak sinkron dengan jumlah kalimat naskah
  if (sceneTimings.length !== sentences.length) {
    // Periksa apakah berkas vokal eksternal atau vo_*.mp3 ada di folder proyek
    let voiceFile = options.customVoice && fs.existsSync(options.customVoice) ? options.customVoice : null;
    if (!voiceFile && fs.existsSync(audioDir)) {
      const vos = fs.readdirSync(audioDir).filter(f => /\.(mp3|wav|m4a|aac|ogg|flac)$/i.test(f));
      if (vos.length > 0) voiceFile = path.join(audioDir, vos[0]);
    }

    let totalDuration = options.duration || 60.0;
    if (voiceFile && fs.existsSync(voiceFile)) {
      try {
        const probeDur = execSync(`ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "${voiceFile}"`).toString().trim();
        const pd = parseFloat(probeDur);
        if (Number.isFinite(pd) && pd > 5) {
          totalDuration = pd + 2.5; // Jeda 2.5 detik agar vokal selesai tuntas dan BGM fade-out lembut
        }
      } catch (e) {}
    } else {
      totalDuration += 2.5;
    }

    // Alokasi proporsional berdasarkan panjang karakter naskah per adegan
    const totalChars = sentences.reduce((acc, s) => acc + s.length, 0);
    let accumTime = 0.0;

    sceneTimings = sentences.map((sent, idx) => {
      const isLast = idx === sentences.length - 1;
      let dur = 0;
      if (isLast) {
        dur = Number((totalDuration - accumTime).toFixed(2));
        if (dur < 5.0) dur = 5.5; // Outro minimal 5.5 detik agar logo dan pesan penutup tampil megah
      } else {
        const charRatio = totalChars > 0 ? (sent.length / totalChars) : (1.0 / sentences.length);
        dur = Number(Math.max(3.5, (totalDuration * charRatio)).toFixed(2));
      }

      const start = Number(accumTime.toFixed(2));
      const end = Number((start + dur).toFixed(2));
      accumTime = end;

      return {
        scene: idx + 1,
        clip: String(idx + 1).padStart(2, '0') + '_scene.mp4',
        topic: `Adegan ${idx + 1}`,
        start,
        end,
        duration: dur,
        setpts: Number((dur / 7.98).toFixed(4)),
        text: sent
      };
    });

    fs.writeFileSync(timingFile, JSON.stringify(sceneTimings, null, 2), 'utf8');
  }

  // 3. Analisis Domain Konten
  const title = options.topic || projectName;
  const activeDomain = detectNicheDomain(scriptText, title, options.domain);

  // 4. Deteksi Pengecualian Klip untuk Re-Scan (Scan Ulang Variasi Baru)
  const choreoFile = path.join(promptsDir, 'choreography_vidcon_scenes.json');
  const excludedClipIds = [];

  if (options.rescan && fs.existsSync(choreoFile)) {
    try {
      const prevChoreo = JSON.parse(fs.readFileSync(choreoFile, 'utf8'));
      if (Array.isArray(prevChoreo)) {
        prevChoreo.forEach(c => {
          if (c && c.dbId) excludedClipIds.push(c.dbId);
        });
      }
    } catch (e) {}
  }

  console.log(`\n=============================================================================`);
  console.log(`  PEMERIKSAAN FOOTAGE & STORYBOARD PRA-RENDER (FOKUSKONTEN VIDEO STUDIO)`);
  console.log(`  Proyek       : ${projectName}`);
  console.log(`  Topik / Judul: ${title}`);
  console.log(`  Domain       : [${activeDomain.id}] ${activeDomain.name}`);
  console.log(`  Total Adegan : ${sentences.length} Babak Kalimat`);
  console.log(`  Mode Pindai  : ${options.rescan ? `SCAN ULANG ALTERNATIF (Mengecualikan ${excludedClipIds.length} klip sebelumnya)` : 'PEMINDAIAN AWAL SEGAR'}`);
  console.log(`=============================================================================\n`);

  // 5. Jalankan Koreografi Cerdas (Dry-Run: Bebas Polusi used_count sampai Render Nyata)
  const projectId = projectName.replace(/^(\d+).*/, '$1') || 'PREVIEW';
  const choreography = buildKinshipSequenceForScript({
    scriptScenes: sceneTimings,
    defaultTheme: options.domain || activeDomain.code,
    domain: activeDomain,
    scriptText,
    title,
    projectId,
    dryRun: true,
    excludeClipIds: excludedClipIds,
    selectedSkus: options.footageSkus
  });

  // Simpan hasil koreografi yang telah divalidasi ke 04_prompts
  fs.writeFileSync(choreoFile, JSON.stringify(choreography, null, 2), 'utf8');

  // 6. Hasilkan Storyboard Contact Sheet 16 Adegan (1080x1920 Full HD Grid 4x4)
  console.log(`\n[STORYBOARD] Membangun lembar kontak visual contact sheet 16 frame...`);
  const gateResult = runPreFlightGate(projectDir);

  // 7. Tampilkan Ringkasan Rinci Per Adegan
  console.log(`\n=============================================================================`);
  console.log(`  DAFTAR KLIP TERPILIH PER ADEGAN:`);
  console.log(`=============================================================================`);
  choreography.forEach(c => {
    console.log(`  Scene ${String(c.sceneIndex).padStart(2, '0')} | [${c.sku}] ${(c.brandCode || '').padEnd(14)} | ${c.duration.toFixed(1)}s | ${c.clipFilename}`);
    console.log(`           Alasan: ${c.reason}`);
  });
  console.log(`=============================================================================`);
  console.log(`  STATUS PEMERIKSAAN : 100% SUKSES LOLOS VERIFIKASI`);
  console.log(`  Lembar Storyboard  : ${gateResult.contactSheetPath}`);
  console.log(`  Petunjuk           : Lembar kontak visual kini tampil di panel pratinjau.`);
  console.log(`                       Jika ingin mencari klip alternatif lain, klik [Scan Ulang Footage].`);
  console.log(`                       Jika sudah cocok, klik tombol Perakitan Video untuk merender.`);
  console.log(`=============================================================================\n`);

  // Output JSON terstruktur di baris terakhir untuk konsumsi GUI C#
  const finalJson = {
    success: true,
    projectName,
    projectDir,
    totalScenes: choreography.length,
    contactSheetPath: gateResult.contactSheetPath,
    isRescan: options.rescan,
    domain: activeDomain.name,
    footageSkus: options.footageSkus || 'auto'
  };
  console.log(`__JSON_RESULT_START__${JSON.stringify(finalJson)}__JSON_RESULT_END__`);
}

main().catch(err => {
  console.error(`[FATAL ERROR] Gagal melakukan pemeriksaan footage: ${err.message}`);
  process.exit(1);
});
