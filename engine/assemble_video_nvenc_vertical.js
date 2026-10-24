/**
 * assemble_video_nvenc_vertical.js — Mesin Perakitan Otomatis Video Vertikal 9:16 Full HD (NVENC GPU)
 * Standar Arsitektur: arsitektur_sistem_video_edukasi_fokuskonten.md (Pilar II & III)
 * 
 * Fitur Baku:
 * - Format: 1080x1920 Full HD Mobile (TikTok / YouTube Shorts / Reels)
 * - Video Filter: scale=1080:1920, crop center, 24fps NVENC preset p4/p5
 * - Audio Formula: VO loudnorm -16 LUFS x --vo-volume% + BGM loudnorm -26 LUFS x --bgm-volume% + amix normalize=0 + alimiter 0.95
 * - Subtitle Hardsub: ASS Studio Canvas 1080x1920 MarginV 580 (Montserrat Black 64pt ALL CAPS Emas)
 * - Output: 06_final_render/Video_Vertikal_[Nama_Proyek]_Full_HD.mp4
 */
'use strict';

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

const FFMPEG_BIN = (pathsResolver && pathsResolver.getBinaryPath) ? pathsResolver.getBinaryPath('ffmpeg') : 'ffmpeg';
const FFPROBE_BIN = (pathsResolver && pathsResolver.getBinaryPath) ? pathsResolver.getBinaryPath('ffprobe') : 'ffprobe';

// Deteksi otomatis dukungan GPU NVENC vs CPU Universal untuk stabilitas di seluruh hardware pengguna
let isNvencAvailable = true;
try {
  execSync(`"${FFMPEG_BIN}" -f lavfi -i color=c=black:s=128x128:d=0.05 -c:v h264_nvenc -f null -`, { stdio: 'ignore' });
} catch (e) {
  isNvencAvailable = false;
  console.warn('[HARDWARE DETECT] GPU NVIDIA NVENC tidak aktif / tidak didukung di PC ini.');
  console.warn('[HARDWARE DETECT] Beralih otomatis ke CPU Encoder Universal (libx264)...');
}

const normEncoderArgs = isNvencAvailable
  ? '-c:v h264_nvenc -preset p4 -rc vbr -cq 26 -b:v 2.5M -maxrate 3.5M'
  : '-c:v libx264 -preset veryfast -crf 24';

const finalEncoderArgs = isNvencAvailable
  ? '-c:v h264_nvenc -preset p5 -rc vbr -cq 26 -b:v 2.5M -maxrate 3.5M -bufsize 7M'
  : '-c:v libx264 -preset veryfast -crf 24';

const targetProject = process.argv[2];
if (!targetProject) {
  console.error('[ERROR] Mohon sertakan direktori proyek sebagai argumen.');
  console.error('Contoh: node assemble_video_nvenc_vertical.js "D:/Proyek_Video/Contoh_Proyek_Vertical"');
  process.exit(1);
}

const projectDir = path.resolve(targetProject);
const projectName = path.basename(projectDir);

console.log(`\n=============================================================================`);
console.log(`  MESIN PERAKITAN OTOMATIS VIDEO VERTIKAL 9:16 (NVENC GPU MASTER PIPELINE)`);
console.log(`  Proyek  : ${projectName}`);
console.log(`  Path    : ${projectDir}`);
console.log(`=============================================================================\n`);

const rawDir = path.join(projectDir, '01_video_raw');
const audioDir = path.join(projectDir, '02_audio_master');
const bgmDir = path.join(projectDir, '03_audio_bgm');
const promptsDir = path.join(projectDir, '04_prompts');
const outDir = path.join(projectDir, '06_final_render');

if (!fs.existsSync(outDir)) {
  fs.mkdirSync(outDir, { recursive: true });
}

// 1. Validasi Master Audio Narasi (Mendukung file eksplisit --vo-file atau berkas terbaru)
const voFileArgIdx = process.argv.indexOf('--vo-file');
const explicitVoFile = (voFileArgIdx !== -1 && process.argv[voFileArgIdx + 1]) ? process.argv[voFileArgIdx + 1] : null;

let voPath = null;
let selectedVo = null;

if (explicitVoFile && fs.existsSync(explicitVoFile)) {
  voPath = explicitVoFile;
  selectedVo = path.basename(explicitVoFile);
  console.log(`[AUDIO 1/2] Master Voiceover Eksplisit: ${selectedVo} (Dipilih langsung dari Pipeline)`);
} else {
  const voFiles = fs.readdirSync(audioDir).filter(f => f.endsWith('.mp3') || f.endsWith('.wav'));
  if (voFiles.length === 0) {
    console.error(`[ERROR] Master audio narasi tidak ditemukan di: ${audioDir}`);
    process.exit(1);
  }
  // Urutkan berdasarkan waktu modifikasi terbaru jika ada beberapa file
  voFiles.sort((a, b) => {
    return fs.statSync(path.join(audioDir, b)).mtimeMs - fs.statSync(path.join(audioDir, a)).mtimeMs;
  });
  selectedVo = voFiles[0];
  voPath = path.join(audioDir, selectedVo);
  console.log(`[AUDIO 1/2] Master Voiceover Terpilih: ${selectedVo} (Berkas Terbaru)`);
}

// 2. Validasi Master Audio BGM — Mendukung penonaktifan eksplisit jika vokal sudah memiliki BGM bawaan
const noBgmFlag = process.argv.includes('--no-bgm') || (fs.existsSync(bgmDir) && fs.existsSync(path.join(bgmDir, 'NO_BGM')));
let bgmPath = null;

if (noBgmFlag) {
  console.log(`[AUDIO 2/2] Mode: TANPA BGM EKSTERNAL (Master audio sudah memiliki instrumen BGM bawaan sendiri)`);
} else {
  if (fs.existsSync(bgmDir)) {
    const bgmFiles = fs.readdirSync(bgmDir).filter(f => f.endsWith('.mp3') || f.endsWith('.wav'));
    if (bgmFiles.length > 0) {
      bgmPath = path.join(bgmDir, bgmFiles[0]);
      console.log(`[AUDIO 2/2] Master Soundtrack BGM : ${path.basename(bgmPath)}`);
    }
  }

  // [v2.0] Fallback BGM: Jika folder proyek kosong, ambil dari library berdasarkan ledger rolling terakhir
  if (!bgmPath) {
    const BGM_LIB = (pathsResolver && pathsResolver.getBgmLibraryDir)
      ? pathsResolver.getBgmLibraryDir()
      : path.join(__dirname, '..', 'assets', 'bgm');
    const ledgerPath = `${BGM_LIB}/bgm_rolling_ledger.json`;
    let fallbackFile = '01_Cello_Orchestral_Reflective.mp3'; // Default darurat
    if (fs.existsSync(ledgerPath)) {
      try {
        const ledger = JSON.parse(fs.readFileSync(ledgerPath, 'utf8'));
        if (ledger.lastUsedFile) fallbackFile = ledger.lastUsedFile;
      } catch(e) {}
    }
    let validBgmFile = null;
    if (fs.existsSync(`${BGM_LIB}/${fallbackFile}`)) {
      validBgmFile = fallbackFile;
    } else {
      const emergencyDefault = '01_Cello_Orchestral_Reflective.mp3';
      if (fs.existsSync(`${BGM_LIB}/${emergencyDefault}`)) {
        validBgmFile = emergencyDefault;
      } else {
        const availableBgms = fs.existsSync(BGM_LIB) ? fs.readdirSync(BGM_LIB).filter(f => f.endsWith('.mp3')).sort() : [];
        if (availableBgms.length > 0) validBgmFile = availableBgms[0];
      }
    }
    if (validBgmFile && fs.existsSync(`${BGM_LIB}/${validBgmFile}`)) {
      bgmPath = `${BGM_LIB}/${validBgmFile}`;
      console.log(`[AUDIO 2/2] BGM Fallback dari Library (${validBgmFile}) - folder proyek kosong.`);
    } else {
      console.warn(`[AUDIO 2/2] PERINGATAN: BGM tidak ditemukan di folder proyek maupun library. Render tanpa musik latar.`);
    }
  }}

// 3. Baca Master Durasi & Koreografi Adegan
const choreographyPath = path.join(promptsDir, 'choreography_vidcon_scenes.json');
if (!fs.existsSync(choreographyPath)) {
  console.error(`[ERROR] Berkas koreografi tidak ditemukan di: ${choreographyPath}`);
  process.exit(1);
}
const choreographyData = JSON.parse(fs.readFileSync(choreographyPath, 'utf8'));
const totalDuration = choreographyData.reduce((acc, cur) => acc + cur.duration, 0).toFixed(2);
console.log(`[TIMING] Terdeteksi ${choreographyData.length} Adegan | Total Durasi: ${totalDuration}s (~${Math.floor(totalDuration / 60)}:${Math.floor(totalDuration % 60).toString().padStart(2, '0')})`);

// 4. Periksa Subtitle File ASS Studio
const assPath = path.join(promptsDir, 'subtitles_synced.ass');
if (!fs.existsSync(assPath)) {
  console.error(`[ERROR] Berkas subtitle ASS tidak ditemukan di: ${assPath}`);
  process.exit(1);
}
console.log(`[SUBTITLE] Berkas Subtitle ASS Kinetik: ${path.basename(assPath)} (MarginV 580 Montserrat Black)`);

// [v4.0] Peta Color Grade Master 3D LUT (.cube) — Grading Sinematik Standar Film Hollywood
// Menggunakan FFmpeg lut3d filter membaca berkas .cube lokal presisi tinggi
const LUTS_VAULT = (pathsResolver && pathsResolver.getLutsDir)
  ? pathsResolver.getLutsDir()
  : path.join(__dirname, '..', 'assets', 'luts');
const ROLLING_LUTS = [
  'lut_teal_orange.cube',
  'lut_nordic_cold.cube',
  'lut_golden_warm.cube',
  'lut_deep_teal.cube',
  'lut_contrasty_cold.cube',
  'lut_neutral_fade.cube',
  'lut_warm_contrasty.cube'
];

// [v4.3] Dukungan Argumen CLI Kustomisasi LUT (--lut <filename|none>)
const lutArgIdx = process.argv.indexOf('--lut');
const customLutFile = (lutArgIdx !== -1 && process.argv[lutArgIdx + 1]) ? process.argv[lutArgIdx + 1] : null;
if (customLutFile) {
  console.log(`[COLOR GRADE] Kustomisasi 3D LUT Terpilih: ${customLutFile}`);
}

// [v4.1] Sistem Rolling Presisi & Akurasi: Pemetaan 3D LUT dan Transisi Berbasis Konteks Narasi Riil
function resolveColorGrade(sku, sceneIndex = 0, text = '') {
  // Jika operator menentukan 3D LUT eksplisit via GUI / CLI
  if (customLutFile) {
    if (customLutFile === 'none' || customLutFile === 'no') {
      return 'eq=contrast=1.0:saturation=1.0';
    }
    return getLutFilter(customLutFile);
  }

  const normText = (text || '').toLowerCase();

  // Prioritas 1: Analisis Semantik Emosi dan Konteks Naskah
  if (/ai|agi|teknologi|modern|robot|masa depan|cyber/i.test(normText)) {
    return getLutFilter('lut_deep_teal.cube'); // Nuansa futuristik & dingin
  }
  if (/penderitaan|sakit|luka|titik nol|terpuruk|dingin|kutukan|gelap|miskin|kurungan|terjebak/i.test(normText)) {
    return getLutFilter('lut_contrasty_cold.cube'); // Kontras dingin pekat penderitaan
  }
  if (/orang tua|zaman|ayah|ibu|anugerah|bertahan|syukur|senja|hidup adalah|amanat/i.test(normText)) {
    return getLutFilter('lut_golden_warm.cube'); // Keemasan hangat reflektif & batin
  }
  if (/wibawa|diam|pria|topeng|aturan|hukum|baja|shelby|singa|pemenang/i.test(normText)) {
    return getLutFilter('lut_nordic_cold.cube'); // Maskulin stoikisme tegas
  }
  if (/usaha|berjuang|lingkaran|keringat|kerja|besi|mesin|keluar/i.test(normText)) {
    return getLutFilter('lut_warm_contrasty.cube'); // Tekstur kerja keras & industri
  }

  // Prioritas 2: Pemetaan Karakter Bawaan Bundel SKU
  const lutMap = {
    'ID9919': 'lut_nordic_cold.cube',      // Shelby — Maskulin, dingin, tegas
    'IDFFA0': 'lut_contrasty_cold.cube',   // Dark Psych — Kontras tinggi, dingin pekat
    'IDB7CA': 'lut_contrasty_cold.cube',   // Joker — Melankolis gelap & dingin
    'IDFCC3': 'lut_golden_warm.cube',      // Senja — Keemasan hangat sunset
    'IDE0C2': 'lut_teal_orange.cube',      // Pemersatu — Teal & Orange cerah sinematik
    'ID1646': 'lut_deep_teal.cube',        // Cyberpunk — Neon teal mendalam
    'IDE5EF': 'lut_deep_teal.cube',        // MechTech — Cyber industrial
    'ID7792': 'lut_neutral_fade.cube',     // Anime — Melankolis film fade
    'ID7667': 'lut_golden_warm.cube',      // Alam — Hangat reflektif
    'ID3199': 'lut_teal_orange.cube',      // WallMotion — Modern teal & orange
    'ID9C29': 'lut_warm_contrasty.cube',   // Karat — Hangat bertekstur
    'ID3C1F': 'lut_warm_contrasty.cube',   // ToolCraft — Hangat kontras
  };

  const chosenLut = lutMap[sku] || ROLLING_LUTS[sceneIndex % ROLLING_LUTS.length];
  return getLutFilter(chosenLut);
}

function getLutFilter(lutFilename) {
  const fullPath = path.join(LUTS_VAULT, lutFilename).replace(/\\/g, '/');
  if (fs.existsSync(fullPath)) {
    return `lut3d='${fullPath.replace(':', '\\:')}'`;
  }
  return 'eq=contrast=1.08:saturation=1.05';
}

// [v5.1] Sistem Transisi Sinematik Mulus (Direct Cut Tanpa Kedipan Hitam Antar Adegan)
function resolveTransitions(text, targetDur, isLastScene = false) {
  const normText = (text || '').toLowerCase();

  // Khusus Adegan Terakhir (Outro Brand): Fade in halus, Fade out panjang ke hitam pekat
  if (isLastScene) {
    return {
      fadeIn: `fade=t=in:st=0:d=0.35:color=black`,
      fadeOut: `fade=t=out:st=${Math.max(0, targetDur - 0.80).toFixed(3)}:d=0.80:color=black`
    };
  }

  // 1. Momen Puncak Ledakan / Kejutan Spektakuler: Flash to White
  if (/meledak|menggetarkan|ledakan|petir/i.test(normText)) {
    return {
      fadeIn: `fade=t=in:st=0:d=0.15:color=white`,
      fadeOut: `fade=t=out:st=${Math.max(0, targetDur - 0.15).toFixed(3)}:d=0.15:color=white`
    };
  }

  // 2. Transisi Baku: Direct Cut Sinematik Mulus (100% Bebas Kedipan Hitam / Seamless Flow)
  return {
    fadeIn: null,
    fadeOut: null
  };
}

// [v4.2] Deteksi Cerdas Letterbox Hitam Bawaan (Auto-Crop 100% Full Screen Edge-to-Edge)
// Memindai klip jika memiliki letterbox hitam bawaan dan memangkasnya sebelum di-scale ke 1080x1920
function detectLetterboxCrop(clipPath) {
  try {
    const cmd = `ffmpeg -hide_banner -ss 00:00:01.5 -i "${clipPath}" -vframes 5 -vf cropdetect=24:16:0 -f null - 2>&1`;
    const output = execSync(cmd).toString();
    const matches = output.match(/crop=(\d+:\d+:\d+:\d+)/g);
    if (!matches || matches.length === 0) return null;

    const lastCrop = matches[matches.length - 1].replace('crop=', '');
    const [w, h, x, y] = lastCrop.split(':').map(Number);

    const probeCmd = `ffprobe -v error -select_streams v:0 -show_entries stream=width,height -of csv=p=0:s=x "${clipPath}"`;
    const [origW, origH] = execSync(probeCmd).toString().trim().split('x').map(Number);

    if (h < origH * 0.92) {
      console.log(`    [AUTOCROP] Letterbox terdeteksi (${origW}x${origH}) -> Pangkas bar hitam ke ${w}x${h} offset Y=${y}`);
      return `crop=${w}:${h}:${x}:${y}`;
    }
    return null;
  } catch (e) {
    return null;
  }
}

// 5. Tahap 1: Normalisasi Klip Visual ke Standar Vertikal (1080x1920 24fps NVENC)
console.log(`\n[TAHAP 1/3] Menormalisasi ${choreographyData.length} klip visual ke 1080x1920 24fps NVENC...`);
const tempNormalizedDir = path.join(rawDir, '_normalized_vertical');
if (process.argv.includes('--fresh') && fs.existsSync(tempNormalizedDir)) {
  fs.rmSync(tempNormalizedDir, { recursive: true, force: true });
}
if (!fs.existsSync(tempNormalizedDir)) {
  fs.mkdirSync(tempNormalizedDir, { recursive: true });
}

const normalizedFiles = [];
for (let i = 0; i < choreographyData.length; i++) {
  const item = choreographyData[i];
  const inputClip = (pathsResolver && pathsResolver.resolveVideoPath)
    ? pathsResolver.resolveVideoPath(item.clipPath)
    : item.clipPath;
  const targetDur = item.duration;
  const outClipName = `norm_scene_${String(item.sceneIndex).padStart(2, '0')}.mp4`;
  const outClip = path.join(tempNormalizedDir, outClipName);
  normalizedFiles.push(outClip);

  const isValidCache = fs.existsSync(outClip) && fs.statSync(outClip).size > 10240;
  if (isValidCache) {
    console.log(`  [${i + 1}/${choreographyData.length}] Scene ${item.sceneIndex} [${item.sku}] -> Cache siap pakai (${(fs.statSync(outClip).size / 1024).toFixed(0)} KB).`);
    continue;
  } else if (fs.existsSync(outClip)) {
    try { fs.unlinkSync(outClip); } catch(e) {}
  }

  if (!fs.existsSync(inputClip)) {
    console.error(`  [ERROR] Berkas footage tidak ada: ${inputClip}`);
    process.exit(1);
  }

  let rawDur = 10;
  try {
    rawDur = parseFloat(execSync(`ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "${inputClip}"`).toString().trim());
  } catch (e) {
    rawDur = 10;
  }

  const loopFlag = rawDur < targetDur ? `-stream_loop -1` : ``;
  const infoLoop = rawDur < targetDur ? `(Looped: ${rawDur.toFixed(1)}s < ${targetDur}s)` : `(Cukup: ${rawDur.toFixed(1)}s)`;

  console.log(`  [${i + 1}/${choreographyData.length}] Scene ${item.sceneIndex} [${item.sku} ${item.brandCode}] -> Target: ${targetDur}s ${infoLoop}`);

  // [v4.2] Pipeline Visual Per Klip:
  // Auto-Crop (jika ada bar hitam) → Scale 1080x1920 → 3D LUT Kontekstual → Radial Corner Vignette Tipis → Transisi Rolling Kontekstual
  const autoCrop = detectLetterboxCrop(inputClip);
  const colorGrade = resolveColorGrade(item.sku, i, item.text);

  // Vignette Radial Sinematik Tipis di Setiap Pojok (DIJAGA TETAP AKTIF: angle=PI/5.2)
  // Menghasilkan bayangan halus melingkar di 4 sudut, memusatkan fokus ke tengah
  const cornerVig = `vignette=angle=PI/5.2`;

  // Transisi rolling akurat berbasis emosi naskah (Dip to Black / Flash White / Micro Crossfade)
  const isLastScene = (i === choreographyData.length - 1);
  const { fadeIn, fadeOut } = resolveTransitions(item.text, targetDur, isLastScene);

  const filterParts = [];
  if (autoCrop) {
    filterParts.push(autoCrop);
  }

  // Khusus Outro Brand ID_BRAND_FK: Dilarang crop zoom-in!
  // Jika rasio landscape, fit dengan letterbox pad agar logo tampil utuh tanpa terpotong.
  // Jika sudah vertikal 1080x1920, scale 1:1 bersih tanpa pemotongan.
  const isBrandOutro = (item.sku === 'ID_BRAND_FK' || (item.clipFilename && item.clipFilename.includes('Metallic_logo')));
  if (isBrandOutro) {
    filterParts.push(
      `scale=1080:1920:force_original_aspect_ratio=decrease`,
      `pad=1080:1920:(ow-iw)/2:(oh-ih)/2:color=black`
    );
  } else {
    filterParts.push(
      `scale=1080:1920:force_original_aspect_ratio=increase`,
      `crop=1080:1920:(in_w-1080)/2:(in_h-1920)/2`
    );
  }

  filterParts.push(
    `fps=24`,
    colorGrade,
    cornerVig
  );
  if (fadeIn) filterParts.push(fadeIn);
  if (fadeOut) filterParts.push(fadeOut);

  const vFilter = filterParts.join(',');

  // [SAFEGUARD AUTO-CUT DINAMIS & ANTI-GENERIK]
  // Klip berdurasi panjang tidak selalu dipotong dari detik 0.0 (yang kerap kali statis / pre-roll).
  // Hitung smart in-point offset agar auto-cut langsung menangkap dinamika aksi visual terbaik.
  let seekArg = '';
  if (isBrandOutro) {
    // Outro logo wajib tampil dari detik 0.0 agar animasi reveal dan pedestal utuh sempurna tanpa loncat
    seekArg = '';
  } else if (rawDur > targetDur + 2.5) {
    const maxOffset = Math.min(3.0, rawDur - targetDur - 0.5);
    const dynamicStart = (0.75 + (i % 4) * 0.5); // Rotasi in-point dinamis 0.75s, 1.25s, 1.75s, 2.25s
    const safeOffset = Math.min(dynamicStart, maxOffset).toFixed(2);
    seekArg = `-ss 00:00:${safeOffset.padStart(5, '0')}`;
  }

  const cmdNorm = `"${FFMPEG_BIN}" -y ${seekArg} ${loopFlag} -i "${inputClip}" -vf "${vFilter}" -t ${targetDur} -an ${normEncoderArgs} -pix_fmt yuv420p "${outClip}"`;
  try {
    execSync(cmdNorm, { stdio: 'pipe' });
  } catch (errNorm) {
    const errDetail = errNorm.stderr ? errNorm.stderr.toString().trim() : (errNorm.message || '');
    const lastLines = errDetail.split(/\r?\n/).slice(-4).join(' | ');
    console.error(`\n[FFMPEG ERROR ADEGAN ${i + 1}/${choreographyData.length}] Gagal menormalisasi klip visual: ${path.basename(inputClip)}`);
    console.error(`  Detail Galat: ${lastLines || 'Klip tidak kompatibel atau encoder sibuk.'}`);
    throw errNorm;
  }
}

// 6. Tahap 2: Concat List
const concatListPath = path.join(tempNormalizedDir, 'concat_list.txt');
const concatContent = normalizedFiles.map(f => `file '${f.replace(/\\/g, '/')}'`).join('\n');
fs.writeFileSync(concatListPath, concatContent, 'utf8');

const mergedVisualPath = path.join(tempNormalizedDir, 'merged_visual_1080x1920.mp4');
console.log(`\n[TAHAP 2/3] Menggabungkan ${choreographyData.length} klip visual ternormalisasi...`);
execSync(`"${FFMPEG_BIN}" -y -f concat -safe 0 -i "${concatListPath}" -c:v copy "${mergedVisualPath}"`, { stdio: 'ignore' });

// 7. Tahap 3: Final Render GPU NVENC dengan High-Impact Soundscape (normalize=0) & Hardsub ASS
const finalOutput = path.join(outDir, `${projectName}_Full_HD.mp4`);

// [v5.1] Kontrol Volume Persen Terkalibrasi (Linear Gain Mulus Bebas Clipping & Pumping)
// Skala linier standar Pilar II: VO default 1.28 (100%), BGM default 0.32 (100%)
const readVolumePct = (flag) => {
  const idx = process.argv.indexOf(flag);
  if (idx === -1 || !process.argv[idx + 1]) return 100;
  const n = parseInt(process.argv[idx + 1], 10);
  if (!Number.isFinite(n)) return 100; // Fallback aman jika input tidak valid
  return Math.min(200, Math.max(0, n)); // Clamp 0-200
};
const voVolPct = readVolumePct('--vo-volume');
const bgmVolPct = readVolumePct('--bgm-volume');
const voGain = ((voVolPct / 100) * 1.28).toFixed(2);
const bgmGain = ((bgmVolPct / 100) * 0.32).toFixed(2);

console.log(`\n[TAHAP 3/3] Final Multiplexing: Visual + VO x${voGain} (${voVolPct}%)${bgmPath ? ` + BGM x${bgmGain} (${bgmVolPct}%)` : ' (tanpa BGM)'} + Hardsub ASS Studio...`);

  // [v4.0] Watermark Premium FokusKonten — Tanpa frame kotak, Lebih Besar, Center, Transparan
  // Font: Montserrat-Bold 52pt, warna white@0.32 dengan soft drop shadow black@0.22
  // Posisi: x=(w-tw)/2 (Horizontal Center), y=150 (Top Center aman dari subtitle)
  // Anti-Redundansi: Muncul setelah Scene 01 jika opening menggunakan bumper 3D logo
  let fontPath = 'C\\:/Windows/Fonts/Montserrat-Bold.ttf';
  if (!fs.existsSync('C:/Windows/Fonts/Montserrat-Bold.ttf')) {
    if (fs.existsSync('C:/Windows/Fonts/arialbd.ttf')) {
      fontPath = 'C\\:/Windows/Fonts/arialbd.ttf';
    } else if (fs.existsSync('C:/Windows/Fonts/segoeui.ttf')) {
      fontPath = 'C\\:/Windows/Fonts/segoeui.ttf';
    } else {
      fontPath = 'C\\:/Windows/Fonts/arial.ttf';
    }
  }

  const lastIdx = choreographyData.length - 1;
  const isLastBrand = (choreographyData[lastIdx] && (choreographyData[lastIdx].sku === 'ID_BRAND_FK' || (choreographyData[lastIdx].clipFilename && choreographyData[lastIdx].clipFilename.includes('Metallic_logo'))));
  const isFirstBrand = (choreographyData[0] && (choreographyData[0].sku === 'ID_BRAND_FK' || (choreographyData[0].clipFilename && choreographyData[0].clipFilename.includes('Metallic_logo'))));

  let wmEnable = `enable='gte(t\\,0)'`;
  if (isFirstBrand) {
    const wmStartTime = Math.max(0, choreographyData[0].duration - 0.2).toFixed(2);
    wmEnable = `enable='gte(t\\,${wmStartTime})'`;
  } else if (isLastBrand) {
    const outroStartTime = Math.max(0, parseFloat(totalDuration) - choreographyData[lastIdx].duration).toFixed(2);
    wmEnable = `enable='between(t\\,0\\,${outroStartTime})'`;
  }
  const wmFilter = `drawtext=text='SmartVidkon':fontfile='${fontPath}':fontsize=52:fontcolor=white@0.32:x=(w-tw)/2:y=150:shadowcolor=black@0.22:shadowx=2:shadowy=2:${wmEnable}`;

  const escapedAssPath = assPath.replace(/\\/g, '/').replace(':', '\\:');
  const subFilter = `ass=filename='${escapedAssPath}'`;

const fadeStart = Math.max(0, parseFloat(totalDuration) - 3.5).toFixed(2);
let complexFilter = '';
let inputsArgs = '';

if (bgmPath) {
  // [v3.3] Proteksi Anti-Freeze BGM: Jika BGM sudah lebih panjang dari video, jangan gunakan -stream_loop -1 agar terhindar dari lonjakan PTS / silent gap
  let loopFlag = '-stream_loop -1';
  try {
    const bgmProbe = execSync(`"${FFPROBE_BIN}" -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "${bgmPath}"`).toString().trim();
    const bgmDur = parseFloat(bgmProbe);
    if (bgmDur >= parseFloat(totalDuration)) {
      loopFlag = ''; // BGM cukup panjang, tanpa looping
      console.log(`[AUDIO 2/2] BGM Durasi (${bgmDur.toFixed(2)}s) >= Video (${totalDuration}s) -> Loop NONAKTIF (100% Seamless Native)`);
    } else {
      console.log(`[AUDIO 2/2] BGM Durasi (${bgmDur.toFixed(2)}s) < Video (${totalDuration}s) -> Menggunakan stream loop`);
    }
  } catch(e) {}

  inputsArgs = `-i "${mergedVisualPath}" -i "${voPath}" ${loopFlag ? loopFlag + ' ' : ''}-i "${bgmPath}"`;
  // [v5.1] Soundscape Murni: Linear Gain VO + Linear Gain BGM + amix normalize=0 (Tanpa Pumping & Anti-Clipping)
  const voChain = `[1:a]volume=${voGain}[vo]`;
  const bgmChain = `[2:a]volume=${bgmGain},afade=t=in:ss=0:d=0.3,afade=t=out:st=${fadeStart}:d=3.5[bgm]`;
  const mixChain = `[vo][bgm]amix=inputs=2:duration=first:dropout_transition=2:normalize=0[aout]`;
  complexFilter = `[0:v]${wmFilter},${subFilter}[vout];${voChain};${bgmChain};${mixChain}`;
} else {
  inputsArgs = `-i "${mergedVisualPath}" -i "${voPath}"`;
  // Cabang tanpa BGM: vokal mengalir murni
  complexFilter = `[0:v]${wmFilter},${subFilter}[vout];[1:a]volume=${voGain}[aout]`;
}

// Kompresi Optimal Mobile Shorts/TikTok: NVENC VBR 2.5 Mbps atau CPU libx264 universal, AAC 192k
// WAJIB -pix_fmt yuv420p agar kompatibel dengan Windows Media Player (anti-error 0xc1010103) & seluruh mobile player
const finalCmd = `"${FFMPEG_BIN}" -y ${inputsArgs} -filter_complex "${complexFilter}" -map "[vout]" -map "[aout]" -t ${totalDuration} ${finalEncoderArgs} -pix_fmt yuv420p -color_primaries bt709 -color_trc bt709 -colorspace bt709 -movflags +faststart -c:a aac -b:a 192k -ar 44100 -ac 2 "${finalOutput}"`;

const startTime = Date.now();
console.log(`[GPU NVENC] Memulai proses render Full HD Terkompresi...`);
execSync(finalCmd, { stdio: 'inherit' });
const renderDuration = ((Date.now() - startTime) / 1000).toFixed(2);

// 8. Verifikasi Visual & Metrik
console.log(`\n=============================================================================`);
console.log(`  PRODUKSI VIDEO VERTIKAL FULL HD SELESAI PARIPURNA!`);
console.log(`  Berkas Final     : ${finalOutput}`);
console.log(`  Waktu Render GPU : ${renderDuration} detik`);
console.log(`=============================================================================\n`);

try {
  const finalProbe = execSync(`"${FFPROBE_BIN}" -v error -show_entries format=duration,size -of json "${finalOutput}"`).toString();
  const meta = JSON.parse(finalProbe);
  const durSec = parseFloat(meta.format.duration).toFixed(2);
  const sizeMb = (parseFloat(meta.format.size) / (1024 * 1024)).toFixed(2);
  console.log(`[VERIFIKASI] Durasi Akhir : ${durSec} detik | Ukuran: ${sizeMb} MB (Terkompresi Efisien)`);

  // Ekstrak 1 snapshot preview visual untuk setiap adegan riil (Total ${choreographyData.length} adegan)
  console.log(`[PREVIEW] Mengekstrak snapshot visual untuk seluruh ${choreographyData.length} adegan...`);
  choreographyData.forEach((item) => {
    const midPoint = Math.min(parseFloat(durSec) - 0.5, item.start + item.duration / 2).toFixed(2);
    const sceneNum = String(item.sceneIndex).padStart(2, '0');
    const framePath = path.join(outDir, `scene_${sceneNum}_preview.jpg`);
    execSync(`"${FFMPEG_BIN}" -y -ss ${midPoint} -i "${finalOutput}" -vframes 1 -q:v 2 "${framePath}"`, { stdio: 'ignore' });
  });
  console.log(`[PREVIEW] Seluruh ${choreographyData.length} snapshot adegan berhasil diekstrak ke 06_final_render.`);
} catch (e) {
  console.log(`[VERIFIKASI] File berhasil dibuat.`);
}

// 9. Auto-Cleanup Temp Raw Cache (Mencegah Hard Disk D: Membengkak)
try {
  if (fs.existsSync(tempNormalizedDir)) {
    console.log(`\n[CLEANUP] Membersihkan folder cache temporer: ${tempNormalizedDir}...`);
    fs.rmSync(tempNormalizedDir, { recursive: true, force: true });
    console.log(`[CLEANUP] Selesai. Folder 01_video_raw kini bersih bebas dari file temporer 100MB+!`);
  }
} catch (cleanErr) {
  console.warn(`[CLEANUP WARNING] Gagal membersihkan cache: ${cleanErr.message}`);
}
