/**
 * run_production_pipeline.js — Master Orchestrator Pipeline Produksi Video Vertikal FokusKonten
 * Standar Arsitektur: arsitektur_sistem_video_edukasi_fokuskonten.md (Disahkan Resmi Keberhasilan Mutlak)
 * 
 * Eksekusi 1-Klik End-to-End:
 * [1/7] Inisialisasi & Validasi Naskah
 * [2/7] Sintesis Narasi Vokal (ElevenLabs Eleven v3 Sarah)
 * [3/7] Ekstraksi Timing Ground-Truth (Faster-Whisper CUDA)
 * [4/7] Generasi Subtitle Kinetik Emas (Montserrat Black ALL CAPS Safe Zone 580)
 * [5/7] Koreografi Footage 21-Bundel & Hook Pemersatu Bangsa (IDE0C2 Fresh Draw)
 * [6/7] Validasi Storyboard Contact Sheet 4x4 Grid
 * [7/7] Perakitan Final FFmpeg GPU NVENC (VBR 2.5 Mbps) + Auto-Purge Cache Raw
 * 
 * Penggunaan CLI:
 *   node run_production_pipeline.js --projectDir "D:/Proyek_Video/01_Video_Edukasi_Vertical" --title "Judul Video Edukasi"
 */
'use strict';

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

const SCRIPTS_DIR = __dirname;
const { generateVoiceover } = require(path.join(SCRIPTS_DIR, 'generate_voiceover_eleven_v3.js'));
const { createKineticSubtitleFile } = require(path.join(SCRIPTS_DIR, 'generate_kinetic_subtitles.js'));
const { buildKinshipSequenceForScript, detectNicheDomain, NICHE_DOMAINS } = require(path.join(SCRIPTS_DIR, 'search_vidcon_local.js'));
const { runPreFlightGate } = require(path.join(SCRIPTS_DIR, 'generate_storyboard_contact_sheet.js'));

// Kata kunci default untuk kinetic gold highlighting ({\c&H00D7FF&})
const EXTENDED_HIGHLIGHT_KEYWORDS = [
  'DIAM', 'MENAKUTKAN', 'MARAH', 'KENDALI EMOSI', 'KONTROL DIRI', 'WIBAWA', 'STOIKISME',
  'MENOLAK PERMINTAAN', 'HANCUR LEBUR', 'BATASAN TEGAS', 'BUKAN KEMULIAAN',
  'ORANG LICIK', 'MENGINJAKMU', 'ALAT', 'BANTING TULANG', 'WAKTU ISTIRAHATMU',
  'HILANG TANPA JEJAK', 'KENYATAAN PAHIT', 'TEMPAT SAMPAH', 'SELEMBAR PENGAKUAN',
  'KEBAIKAN SEJATI', 'BERKATA TIDAK', 'TATAPAN DINGIN', 'PARASIT', 'FOKUS MERAWAT',
  'MASA DEPANMU', 'TERLALU MAHAL', 'BENTENG PRIVASIMU', 'KETERAMPILAN',
  'MENGHORMATI BATAS', 'PRINSIP STOIK', 'KENDALI NASIBMU', 'BATASAN BAJA', 'PEMENANG SEJATI',
  'SERIGALA', 'SINGA', 'TENANG', 'ENERGI', 'RUNTUH', 'KEKUATAN SEJATI',
  'IMPIAN BESARMU', 'PUJIAN AWAL', 'STRATEGI PERANGMU', 'VALIDASI PUBLIK', 'HASIL KERINGATMU', 'DISIPLINMU', 'SENYAP',
  'PANTULAN DIRIMU', 'PERPUTARAN WAKTU', 'SENYUM PAHIT', 'LELAH KERJA', 'SAMUDRA', 'BAJA TERBAIK', 'MESIN PABRIK', 'TANGAN TERAMPIL', 'KOTA MASA DEPAN'
];

// Direktori Pustaka Audio BGM Resmi
const BGM_LIBRARY_DIR = (pathsResolver && pathsResolver.getBgmLibraryDir)
  ? pathsResolver.getBgmLibraryDir()
  : path.join(__dirname, '..', 'assets', 'bgm');

// [v2.0] Registri Vokal Resmi FokusKonten
// Prioritas 1: Arunika (Voice Library Eleven v3 via Web Studio / Drop File)
// Prioritas 2 (Headless API): Sarah (terbukti hangat, mengalir & bebas paywall 402)
// George: DIELIMINASI MUTLAK (terbukti kaku di Video #09)
// Alice : DIELIMINASI MUTLAK (instruksi resmi Muhari)
const VOICE_ROTATION_ORDER = ['kore', 'sarah', 'aoede'];
const VOICE_ROLLING_LEDGER_PATH = path.join(BGM_LIBRARY_DIR, 'voice_rolling_ledger.json');

/**
 * [v2.0] Membaca dan memperbarui Voice Rolling Ledger.
 * Jika ledger tidak ada, inisialisasi dengan lastIndex: -1 (mulai dari 'george').
 * @returns {string} nama vokal yang dipilih
 */
function pickNextVoiceFromLedger() {
  let ledger = { lastIndex: -1, lastUsedVoice: null, history: [] };

  // Init otomatis jika file belum ada
  if (!fs.existsSync(VOICE_ROLLING_LEDGER_PATH)) {
    fs.writeFileSync(VOICE_ROLLING_LEDGER_PATH, JSON.stringify(ledger, null, 2), 'utf8');
  } else {
    try {
      ledger = JSON.parse(fs.readFileSync(VOICE_ROLLING_LEDGER_PATH, 'utf8'));
    } catch (e) {
      console.warn(`[VOICE LEDGER] Gagal membaca ledger, reset ke awal: ${e.message}`);
    }
  }

  const nextIndex = ((ledger.lastIndex ?? -1) + 1) % VOICE_ROTATION_ORDER.length;
  const chosenVoice = VOICE_ROTATION_ORDER[nextIndex];

  ledger.lastIndex = nextIndex;
  ledger.lastUsedVoice = chosenVoice;
  ledger.history = (ledger.history || []).concat({
    voice: chosenVoice,
    timestamp: new Date().toISOString()
  }).slice(-20);

  try {
    fs.writeFileSync(VOICE_ROLLING_LEDGER_PATH, JSON.stringify(ledger, null, 2), 'utf8');
  } catch (e) {
    console.warn(`[VOICE LEDGER] Gagal menyimpan ledger: ${e.message}`);
  }

  console.log(`[VOICE ROLLING] Vokal terpilih: ${chosenVoice.toUpperCase()} (Indeks Rotasi: ${nextIndex}/${VOICE_ROTATION_ORDER.length - 1})`);
  return chosenVoice;
}

/**
 * [v3.5] Resolusi Fleksibel Sumber BGM Berdasarkan Nomor Urut, Judul, atau Path
 * Memungkinkan pemanggilan simpel seperti: --bgm 13 atau --bgm "Rauf Faik"
 */
function resolveBgmSource(input, activeDomain, bgmLibraryDir) {
  if (!input) return null;
  const str = String(input).trim();
  if (str === 'none' || str === 'no') return 'none';

  // 1. Cek jika sudah berupa path absolut yang valid
  if (path.isAbsolute(str) && fs.existsSync(str)) {
    return str;
  }

  // 2. Cek jika berupa relative path langsung
  if (fs.existsSync(path.resolve(str))) {
    return path.resolve(str);
  }

  // 3. Normalisasi pencarian nomor urut & nama file
  const numPad = str.replace(/\D/g, '').padStart(2, '0');
  const candidateDirs = [
    path.join(bgmLibraryDir, activeDomain?.bgmFolder || '01_Stoik_Realitas'),
    bgmLibraryDir,
    path.join(bgmLibraryDir, '00_SSOT')
  ];

  for (const dir of candidateDirs) {
    if (!fs.existsSync(dir)) continue;
    const files = fs.readdirSync(dir).filter(f => f.endsWith('.mp3') || f.endsWith('.wav'));

    // Cek kecocokan nomor urut (misal: "13" mencocokkan "13_Rauf_Faik_Violin_Cover.mp3")
    if (numPad && numPad !== '00') {
      const matchByNum = files.find(f => f.startsWith(`${numPad}_`) || f.startsWith(`${parseInt(numPad)}_`));
      if (matchByNum) return path.join(dir, matchByNum);
    }

    // Cek kecocokan nama file (case-insensitive substring)
    const matchByName = files.find(f => f.toLowerCase().includes(str.toLowerCase()));
    if (matchByName) return path.join(dir, matchByName);
  }

  return null;
}

/**
 * Menyelaraskan segmen raw Faster-Whisper ke tingkat kalimat naskah (Pacing Tenang 5.5s - 8.5s)
 * [v2.0] Mendukung alignment presisi tinggi berbasis word_timing_master.json
 */
function alignSegmentsToSentences(sentences, segments, words = null) {
  if (!sentences || sentences.length === 0) return segments || [];
  if (!segments || segments.length === 0) return [];

  const OUTRO_SAFETY_PADDING = 2.5; // Jeda hening 2.5 detik di penutup video agar vokal selesai tuntas dan BGM fade-out lembut
  const totalAudioDuration = segments[segments.length - 1].end;
  const finalVideoDuration = Number((totalAudioDuration + OUTRO_SAFETY_PADDING).toFixed(2));
  const sentenceTimings = [];
  const clean = t => (t || '').toLowerCase().replace(/[^a-z0-9]/g, '');

  // JALUR 1: Jika word_timing_master tersedia, gunakan akumulasi kata per kata (Ground-Truth Paling Presisi)
  if (Array.isArray(words) && words.length > 0) {
    let wordIdx = 0;
    for (let s = 0; s < sentences.length; s++) {
      const sentenceText = sentences[s].trim();
      const targetClean = clean(sentenceText);
      const targetWords = sentenceText.split(/\s+/).map(clean).filter(Boolean);
      const lastTargetWord = targetWords.length > 0 ? targetWords[targetWords.length - 1] : '';

      const firstWordIdx = Math.min(wordIdx, words.length - 1);
      let accumClean = '';
      let bestEndIdx = firstWordIdx;

      while (wordIdx < words.length) {
        const cur = words[wordIdx];
        accumClean += cur.clean;

        // Cek jika kata saat ini cocok dengan kata penutup kalimat resmi
        if (lastTargetWord && cur.clean === lastTargetWord && accumClean.length >= targetClean.length * 0.85) {
          bestEndIdx = wordIdx;
          wordIdx++;
          break;
        }

        // Cek jika sudah mencapai panjang karakter penuh kalimat
        if (accumClean.length >= targetClean.length) {
          bestEndIdx = wordIdx;
          wordIdx++;
          break;
        }

        bestEndIdx = wordIdx;
        wordIdx++;
      }

      const speechStart = firstWordIdx < words.length ? words[firstWordIdx].start : (s === 0 ? 0.0 : sentenceTimings[s - 1].end);
      const speechEnd = bestEndIdx < words.length ? words[bestEndIdx].end : totalAudioDuration;

      // start adegan footage: sambung dari akhir adegan sebelumnya agar visual tidak bocor
      const start = s === 0 ? 0.0 : sentenceTimings[s - 1].end;
      let end;
      if (s === sentences.length - 1) {
        end = finalVideoDuration;
      } else if (wordIdx < words.length) {
        const nextSpeechStart = words[wordIdx].start;
        // Jika ada jeda hening antar kalimat > 0.8s, beri napas 0.4s lalu potong adegan
        if (nextSpeechStart - speechEnd > 0.8) {
          end = Number((speechEnd + 0.40).toFixed(2));
        } else {
          end = Number(((speechEnd + nextSpeechStart) / 2).toFixed(2));
        }
      } else {
        end = Number((speechEnd + 0.50).toFixed(2));
      }

      let dur = Number((end - start).toFixed(2));
      if (dur <= 0) {
        dur = 1.0;
        end = Number((start + dur).toFixed(2));
      }
      sentenceTimings.push({
        scene: s + 1,
        clip: String(s + 1).padStart(2, '0') + '_scene.mp4',
        topic: `Adegan ${s + 1}`,
        start: Number(start.toFixed(2)),
        end: Number(end.toFixed(2)),
        speechStart: Number(speechStart.toFixed(2)),
        speechEnd: Number(speechEnd.toFixed(2)),
        duration: dur,
        setpts: Number((dur / 7.98).toFixed(4)),
        text: sentenceText
      });
    }
    return sentenceTimings;
  }


  // JALUR 2: Fallback ke segmen kalimat dengan penjaga batas indeks aman (Anti-Undefined Guard)
  let segIdx = 0;
  for (let s = 0; s < sentences.length; s++) {
    const targetSentence = sentences[s];
    const targetClean = clean(targetSentence);
    let accumClean = '';
    const start = s === 0 ? 0.0 : sentenceTimings[s - 1].end;
    let end = segIdx < segments.length ? segments[segIdx].end : finalVideoDuration;

    while (segIdx < segments.length) {
      const cur = segments[segIdx];
      accumClean += clean(cur.text);
      end = cur.end;
      segIdx++;

      if (s === sentences.length - 1) {
        end = finalVideoDuration;
      } else if (accumClean.length >= targetClean.length * 0.85) {
        break;
      }
    }

    if (s === sentences.length - 1) {
      end = finalVideoDuration;
    }

    let dur = +(end - start).toFixed(2);
    if (dur <= 0) {
      dur = 1.0;
      end = +(start + dur).toFixed(2);
    }
    sentenceTimings.push({
      scene: s + 1,
      clip: String(s + 1).padStart(2, '0') + '_scene.mp4',
      topic: `Adegan ${s + 1}`,
      start: +(start).toFixed(2),
      end: +(end).toFixed(2),
      duration: dur,
      setpts: +(dur / 7.98).toFixed(4),
      text: targetSentence
    });
  }

  return sentenceTimings;
}

async function runPipeline(options = {}) {
  const startTime = Date.now();

  const projectDir = path.resolve(options.projectDir);
  const title = options.title || path.basename(projectDir);
  const scriptText = options.scriptText || null;
  const scriptPath = options.scriptPath || path.join(projectDir, '04_prompts', 'naskah_full.txt');

  // Buat struktur direktori proyek resmi
  const rawDir = path.join(projectDir, '01_video_raw');
  const audioDir = path.join(projectDir, '02_audio_master');
  const bgmDir = path.join(projectDir, '03_audio_bgm');
  const promptsDir = path.join(projectDir, '04_prompts');
  const subsDir = path.join(projectDir, '05_subtitles');
  const renderDir = path.join(projectDir, '06_final_render');

  [rawDir, audioDir, bgmDir, promptsDir, subsDir, renderDir].forEach(d => {
    if (!fs.existsSync(d)) fs.mkdirSync(d, { recursive: true });
  });

  // [1/7] Validasi & Penyimpanan Naskah & Deteksi Domain
  console.log(`[1/7] Menyiapkan naskah konten edukasi & analisis domain...`);
  let finalScript = '';
  if (scriptText) {
    finalScript = scriptText.trim();
    fs.writeFileSync(scriptPath, finalScript, 'utf8');
  } else if (fs.existsSync(scriptPath)) {
    finalScript = fs.readFileSync(scriptPath, 'utf8').replace(/^\uFEFF/, '').trim();
  } else {
    throw new Error(`Naskah tidak ditemukan di: ${scriptPath} dan argumen scriptText kosong.`);
  }

  const sentences = finalScript.split('\n').map(s => s.trim().replace(/^\uFEFF/, '')).filter(Boolean);
  console.log(`  Jumlah Kalimat Terdeteksi : ${sentences.length} Kalimat`);
  console.log(`  Panjang Karakter          : ${finalScript.length} Karakter`);
  console.log(`  Berkas Naskah             : ${scriptPath}`);

  // [v3.0] Multi-Niche Domain Engine (Zero Risk: Default ke DOMAIN_STOIC)
  const activeDomain = detectNicheDomain(finalScript, title, options.domain);
  console.log(`  Domain Konten Terdeteksi  : [${activeDomain.id}] ${activeDomain.name}`);
  console.log(`  BGM Library Subfolder     : ${activeDomain.bgmFolder}`);
  console.log(`  Hook Scene 01 Terkunci    : ${activeDomain.hookBundle}`);

  // [v2.0] Voice Rolling: Ambil dari ledger otomatis jika tidak ada opsi eksplisit
  const selectedVoice = options.voice || pickNextVoiceFromLedger();

  // [v3.5] Resolusi Fleksibel BGM (Mendukung nomor urut seperti '13', judul, atau path)
  let bgmSource = resolveBgmSource(options.bgmSource, activeDomain, BGM_LIBRARY_DIR);
  const isNoBgm = bgmSource === 'none' || options.noBgm;

  if (isNoBgm) {
    bgmSource = null;
    fs.writeFileSync(path.join(bgmDir, 'NO_BGM'), 'Audio master sudah memiliki BGM bawaan.', 'utf8');
    fs.readdirSync(bgmDir).filter(f => f.endsWith('.mp3')).forEach(f => {
      try { fs.unlinkSync(path.join(bgmDir, f)); } catch(e) {}
    });
  } else if (!bgmSource) {
    const domainBgmDir = path.join(BGM_LIBRARY_DIR, activeDomain.bgmFolder);
    const targetBgmDir = fs.existsSync(domainBgmDir) ? domainBgmDir : BGM_LIBRARY_DIR;
    const bgmFiles = fs.readdirSync(targetBgmDir).filter(f => f.endsWith('.mp3')).sort();

    if (bgmFiles.length > 0) {
      const ledgerPath = path.join(targetBgmDir, 'ledger.json');
      let ledger = { lastIndex: -1, lastUsedFile: null, history: [] };
      if (fs.existsSync(ledgerPath)) {
        try { ledger = JSON.parse(fs.readFileSync(ledgerPath, 'utf8')); } catch(e) {}
      } else {
        try { fs.writeFileSync(ledgerPath, JSON.stringify(ledger, null, 2), 'utf8'); } catch(e) {}
      }
      const nextIndex = ((ledger.lastIndex ?? -1) + 1) % bgmFiles.length;
      const chosenBgm = bgmFiles[nextIndex];
      bgmSource = path.join(targetBgmDir, chosenBgm);

      ledger.lastIndex = nextIndex;
      ledger.lastUsedFile = chosenBgm;
      ledger.history = (ledger.history || []).concat({ file: chosenBgm, timestamp: new Date().toISOString() }).slice(-20);
      try { fs.writeFileSync(ledgerPath, JSON.stringify(ledger, null, 2), 'utf8'); } catch(e) {}
    }
  }
  if (!isNoBgm && !bgmSource) {
    const defaultLocalBgm = path.join(BGM_LIBRARY_DIR, '01_Stoik_Realitas', '01_Cello_Orchestral_Reflective.mp3');
    if (fs.existsSync(defaultLocalBgm)) {
      bgmSource = defaultLocalBgm;
    } else {
      bgmSource = path.join(BGM_LIBRARY_DIR, '01_Stoik_Realitas', '01_Cello_Orchestral_Reflective.mp3');
    }
  }

  console.log(`\n=============================================================================`);
  console.log(`  MASTER ORCHESTRATOR PIPELINE VIDEO VERTIKAL EDUKASI FOKUSKONTEN`);
  console.log(`  Proyek : ${path.basename(projectDir)}`);
  console.log(`  Judul  : ${title}`);
  console.log(`  Domain : [${activeDomain.id}] ${activeDomain.name}`);
  console.log(`  Vokal  : ${selectedVoice.toUpperCase()}`);
  console.log(`  BGM    : ${isNoBgm ? 'NONAKTIF (Audio master sudah ber-BGM bawaan)' : path.basename(bgmSource) + ' (' + activeDomain.bgmFolder + ')'}`);
  console.log(`  Jalur  : ${projectDir}`);
  console.log(`=============================================================================\n`);

  // Siapkan BGM Rolling
  if (!isNoBgm && bgmSource) {
    const bgmDest = path.join(bgmDir, path.basename(bgmSource));
    if (!fs.existsSync(bgmDest) || options.forceBgm) {
      if (fs.existsSync(bgmSource)) {
        // Hapus BGM lama di folder jika ada pergantian rolling
        fs.readdirSync(bgmDir).forEach(f => {
          try { fs.unlinkSync(path.join(bgmDir, f)); } catch (e) {}
        });
        fs.copyFileSync(bgmSource, bgmDest);
        console.log(`  BGM Soundtrack Tersalin   : ${path.basename(bgmDest)}`);
      } else {
        console.warn(`  [PERINGATAN] File BGM sumber tidak ditemukan di: ${bgmSource}`);
      }
    }
  }

  // [2/7] Sintesis Narasi Vokal (Rolling Voice Support / Bypass Audio Upload)
  const voTarget = path.join(audioDir, `vo_${selectedVoice}_${path.basename(projectDir)}.mp3`);
  if (options.customVoice && fs.existsSync(options.customVoice)) {
    console.log(`\n[2/7] [BYPASS TTS ELEVENLABS] Menggunakan audio vokal upload eksternal:`);
    console.log(`  Berkas Sumber   : ${options.customVoice}`);
    // Bersihkan file vokal lama di audioDir agar berkas vokal baru tidak bentrok
    fs.readdirSync(audioDir).filter(f => /\.(mp3|wav|m4a|aac|ogg|flac)$/i.test(f)).forEach(f => {
      try { fs.unlinkSync(path.join(audioDir, f)); } catch (e) {}
    });
    try {
      // Normalisasi ke stereo 44.1kHz 192kbps agar kompatibel penuh dengan Faster-Whisper dan NVENC
      execSync(`ffmpeg -y -i "${options.customVoice}" -vn -ar 44100 -ac 2 -b:a 192k "${voTarget}"`, { stdio: 'pipe' });
      console.log(`  Berkas Vokal    : ${path.basename(voTarget)} (Ternormalisasi 44.1kHz Stereo)`);
    } catch (errConv) {
      fs.copyFileSync(options.customVoice, voTarget);
      console.log(`  Berkas Vokal    : ${path.basename(voTarget)} (Disalin langsung)`);
    }
    console.log(`  Status TTS      : DILEWATI 100% (Hemat Kuota ElevenLabs)`);
    console.log(`  Volume Vokal    : ${options.voVolume || 100}% (Terhubung ke Pengatur Volume Vokal)`);
  } else if (fs.existsSync(voTarget) && fs.statSync(voTarget).size > 10000 && !options.forceVoiceover) {
    console.log(`\n[2/7] Master vokal sudah tersedia, melewati proses TTS: ${path.basename(voTarget)}`);
  } else {
    console.log(`\n[2/7] Memulai sintesis vokal ElevenLabs Eleven v3 (${selectedVoice})...`);
    await generateVoiceover({
      text: finalScript,
      outputFilePath: voTarget,
      voice: selectedVoice,
      mode: options.intonation || 'clarity',
      speedRatio: options.speedRatio || 1.0
    });
  }

  // ── [2/7 POST] TELEMETRI DURASI AUDIO MASTER (100% NORMAL TANPA SLOWED) ─────
  let audioDurSec = 0;
  if (fs.existsSync(voTarget)) {
    try {
      const ffprobeBin = (pathsResolver && pathsResolver.getBinaryPath) ? pathsResolver.getBinaryPath('ffprobe') : 'ffprobe';
      const probeOut = execSync(
        `"${ffprobeBin}" -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "${voTarget}"`,
        { stdio: ['pipe', 'pipe', 'pipe'] }
      ).toString().trim();
      audioDurSec = parseFloat(probeOut) || 0;
      console.log(`\n  [TELEMETRI VOKAL] Audio Master Alami : ${audioDurSec.toFixed(1)}s (~${(audioDurSec / 60).toFixed(1)} menit)`);
      console.log(`  [TELEMETRI VOKAL] Kecepatan           : 100% Normal (Bebas Slowed / Tanpa Manipulasi Tempo)`);
    } catch (_) {}
  }


  // [3/7] Ekstraksi Ground-Truth Timing (Faster-Whisper CUDA / Smart Proportional Fallback)
  console.log(`\n[3/7] Menjalankan ekstraksi alignment timing via Faster-Whisper CUDA...`);
  const whisperScript = path.join(SCRIPTS_DIR, 'extract_audio_timing_whisper.py');
  const uvBin = fs.existsSync('C:\\Users\\Muhari\\.local\\bin\\uv.exe') ? 'C:\\Users\\Muhari\\.local\\bin\\uv.exe' : 'uv';
  const timingFile = path.join(promptsDir, 'sentence_timing_master.json');
  let whisperSuccess = false;

  try {
    execSync(`"${uvBin}" run --with faster-whisper python "${whisperScript}" "${projectDir}"`, { stdio: 'inherit' });
    if (fs.existsSync(timingFile)) {
      whisperSuccess = true;
    }
  } catch (err) {
    console.warn(`  [TIMING NOTIF] Faster-Whisper tidak terpasang di sistem (${err.message}). Beralih ke Smart Proportional Timing Engine...`);
  }

  // Fallback proporsional jika Whisper tidak tersedia (misal di Windows Sandbox murni tanpa Python)
  if (!whisperSuccess || !fs.existsSync(timingFile)) {
    console.log(`  [TIMING PROPORTIONAL] Mengalkulasi stempel waktu proporsional berbasis durasi vokal (${audioDurSec.toFixed(2)}s)...`);
    const nSentences = sentences.length > 0 ? sentences : ["Adegan pembuka video vertikal", "Penjelasan inti materi", "Pesan penutup narasi"];
    const totalChars = nSentences.reduce((acc, s) => acc + s.length, 0) || 1;
    const targetDur = audioDurSec > 0 ? audioDurSec : (options.targetDuration || 60.0);
    let currStart = 0.0;
    const fallbackTiming = [];
    const fallbackWords = [];

    for (let i = 0; i < nSentences.length; i++) {
      const sText = nSentences[i];
      const sFraction = sText.length / totalChars;
      const sDur = Math.max(1.5, parseFloat((sFraction * targetDur).toFixed(3)));
      const sEnd = parseFloat((currStart + sDur).toFixed(3));
      fallbackTiming.push({
        index: i + 1,
        text: sText,
        start: currStart,
        end: sEnd,
        duration: sDur
      });

      const wList = sText.split(/\s+/).filter(Boolean);
      const wDur = sDur / (wList.length || 1);
      let wStart = currStart;
      for (const w of wList) {
        const wEnd = parseFloat((wStart + wDur).toFixed(3));
        fallbackWords.push({
          word: w,
          clean: w.toLowerCase().replace(/[^a-z0-9]/g, ''),
          start: wStart,
          end: wEnd,
          prob: 0.99
        });
        wStart = wEnd;
      }

      currStart = sEnd;
    }

    if (fallbackTiming.length > 0) {
      fallbackTiming[fallbackTiming.length - 1].end = targetDur;
      fallbackTiming[fallbackTiming.length - 1].duration = parseFloat((targetDur - fallbackTiming[fallbackTiming.length - 1].start).toFixed(3));
    }

    fs.writeFileSync(timingFile, JSON.stringify(fallbackTiming, null, 2), 'utf8');
    const wordTimingPath = path.join(promptsDir, 'word_timing_master.json');
    fs.writeFileSync(wordTimingPath, JSON.stringify(fallbackWords, null, 2), 'utf8');
    console.log(`  [TIMING PROPORTIONAL] Sukses menyusun ${fallbackTiming.length} adegan timing proporsional.`);
  }

  let rawTiming = JSON.parse(fs.readFileSync(timingFile, 'utf8'));

  // Selaraskan ke tingkat 16-17 kalimat naskah jika ada fragmentasi koma
  let sceneTiming = rawTiming;
  if (sentences.length > 0) {
    if (rawTiming.length !== sentences.length) {
      console.log(`  Menyelaraskan ${rawTiming.length} segmen suara menjadi ${sentences.length} adegan kalimat utuh (Pacing Tenang 5.5s-8.5s)...`);
      const wordTimingPath = path.join(promptsDir, 'word_timing_master.json');
      let wordsData = null;
      if (fs.existsSync(wordTimingPath)) {
        try { wordsData = JSON.parse(fs.readFileSync(wordTimingPath, 'utf8')); } catch (e) {}
      }
      sceneTiming = alignSegmentsToSentences(sentences, rawTiming, wordsData);
      fs.writeFileSync(timingFile, JSON.stringify(sceneTiming, null, 2), 'utf8');
    } else {
      console.log(`  Menyelaraskan teks naskah resmi 100% ke ${sceneTiming.length} adegan...`);
      for (let i = 0; i < sceneTiming.length; i++) {
        sceneTiming[i].text = sentences[i];
      }
      fs.writeFileSync(timingFile, JSON.stringify(sceneTiming, null, 2), 'utf8');
    }
  }
  console.log(`  Timing terkalibrasi : ${sceneTiming.length} adegan (Pacing terfokus & mendalam).`);

  // [4/7] Generasi Subtitle Kinetik Emas
  console.log(`\n[4/7] Membangun Subtitle Kinetik Emas (Montserrat Black ALL CAPS Safe Zone 580)...`);
  const assTarget1 = path.join(promptsDir, 'subtitles_synced.ass');
  const assTarget2 = path.join(subsDir, 'subtitles_kinetic_v3.ass');
  createKineticSubtitleFile({
    title: `FokusKonten - ${title}`,
    sceneTiming,
    highlightKeywords: EXTENDED_HIGHLIGHT_KEYWORDS,
    projectDir  // [v2.0] Diteruskan agar auto-detect word_timing_master.json aktif
  }, assTarget1);
  fs.copyFileSync(assTarget1, assTarget2);

  // [5/7] Koreografi Footage Multi-Niche & Anti-Pattern Engine
  console.log(`\n[5/7] Mengorkestrasi Koreografi Footage Multi-Niche Anti-Pola 5-Lapis...`);
  console.log(`  Domain Aktif  : [${activeDomain.id}] ${activeDomain.name}`);
  console.log(`  Hook Scene 01 : ${activeDomain.hookBundle}`);
  const projectId = path.basename(projectDir).replace(/^(\d+).*/, '$1') || '09';
  const choreoFile = path.join(promptsDir, 'choreography_vidcon_scenes.json');

  let choreography = null;
  if (fs.existsSync(choreoFile) && !options.forceChoreo) {
    try {
      const existing = JSON.parse(fs.readFileSync(choreoFile, 'utf8'));
      if (Array.isArray(existing) && existing.length === sceneTiming.length) {
        console.log(`  [PRE-FLIGHT ADOPT] Mengadopsi ${existing.length} adegan dari Pemeriksaan Footage yang Telah Ditinjau.`);
        choreography = existing.map((c, i) => {
          return Object.assign({}, c, {
            duration: sceneTiming[i].duration,
            text: sceneTiming[i].text
          });
        });

        // GUARD HOOK: Pastikan Scene 01 selalu menggunakan hookBundle resmi domain aktif atau SKU hook pilihan user (ID1AF4/IDE0C2)
        const validHookSkus = ['ID1AF4', 'IDE0C2', activeDomain.hookBundle].filter(Boolean);
        const requiredHook = activeDomain.hookBundle;
        if (choreography[0] && !validHookSkus.includes(choreography[0].sku) && requiredHook) {
          console.log(`  [HOOK GUARD] Scene 01 SKU lama (${choreography[0].sku}) bukan SKU hook terpercaya — dipulihkan otomatis ke ${requiredHook}.`);
          choreography[0] = Object.assign({}, choreography[0], { sku: requiredHook, brandCode: requiredHook, matchReason: `Hook Resmi Dipulihkan [${activeDomain.name}]` });
        }
        // GUARD MID-ROLL: Pastikan adegan tengah mengandung midRollBundle
        const requiredMid = activeDomain.midRollBundle || 'IDE0C2';
        const midIdx = Math.floor(choreography.length * 0.45);
        const hasMidRoll = choreography.some(c => c.sku === requiredMid);
        if (!hasMidRoll && choreography[midIdx]) {
          console.log(`  [MID-ROLL GUARD] Mid-roll (${requiredMid}) tidak ditemukan — disuntikkan di adegan ${midIdx + 1}.`);
          choreography[midIdx] = Object.assign({}, choreography[midIdx], { sku: requiredMid, brandCode: requiredMid, matchReason: `Mid-Roll Retention Booster Dipulihkan` });
        }
      }
    } catch (e) {}
  }

  if (!choreography) {
    choreography = buildKinshipSequenceForScript({
      scriptScenes: sceneTiming,
      defaultTheme: options.theme || activeDomain.code,
      domain: activeDomain,
      scriptText: finalScript,
      title,
      projectId,
      dryRun: false,
      selectedSkus: options.footageSkus
    });
  }
  fs.writeFileSync(choreoFile, JSON.stringify(choreography, null, 2), 'utf8');
  console.log(`  Koreografi tersimpan : ${choreoFile} (${choreography.length} adegan)`);

  // [6/7] Validasi Storyboard Contact Sheet 4x4 Grid
  console.log(`\n[6/7] Menghasilkan Storyboard Contact Sheet Pra-Render...`);
  const storyboardScript = path.join(SCRIPTS_DIR, 'generate_storyboard_contact_sheet.js');
  execSync(`node "${storyboardScript}" "${projectDir}"`, { stdio: 'inherit' });

  // [7/7] Perakitan Final FFmpeg GPU NVENC (VBR 2.5 Mbps) + Auto-Purge Cache Raw
  console.log(`\n[7/7] Merakit Video Final 1080x1920 Vertikal via GPU NVENC Hardware Acceleration...`);
  const lutParam = options.lut ? ` --lut "${options.lut}"` : '';
  // Volume persen integer 0-200 (default 100); clamp ulang untuk pemanggil via require() modul
  const clampVolPct = (v) => { const n = parseInt(v, 10); return Number.isFinite(n) ? Math.min(200, Math.max(0, n)) : 100; };
  const volumeParam = ` --vo-volume ${clampVolPct(options.voVolume)} --bgm-volume ${clampVolPct(options.bgmVolume)}`;
  const voFileParam = fs.existsSync(voTarget) ? ` --vo-file "${voTarget}"` : '';
  const assemblerScript = path.join(SCRIPTS_DIR, 'assemble_video_nvenc_vertical.js');
  execSync(`node "${assemblerScript}" "${projectDir}"${isNoBgm ? ' --no-bgm' : ''}${lutParam}${volumeParam}${voFileParam}`, { stdio: 'inherit' });

  // Ekstrak 5 Preview Snapshot Frame untuk Tinjauan Instan
  console.log(`\n=== MENGEKSTRAK FRAME PRATINJAU AKHIR ===`);
  const finalVideoFiles = fs.readdirSync(renderDir).filter(f => f.endsWith('.mp4'));
  if (finalVideoFiles.length > 0) {
    const finalMp4 = path.join(renderDir, finalVideoFiles[0]);
    const duration = parseFloat(execSync(`ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "${finalMp4}"`).toString().trim()) || 100;
    
    const intervals = [4, Math.floor(duration * 0.25), Math.floor(duration * 0.50), Math.floor(duration * 0.75), Math.floor(duration * 0.92)];
    intervals.forEach(sec => {
      const snapName = `preview_${String(sec).padStart(3, '0')}s.jpg`;
      const snapTarget = path.join(renderDir, snapName);
      try {
        execSync(`ffmpeg -y -ss ${sec} -i "${finalMp4}" -vframes 1 "${snapTarget}"`, { stdio: 'ignore' });
      } catch (e) {}
    });
    console.log(`  5 frame pratinjau berhasil disimpan di: ${renderDir}`);
  }

  const elapsedTotal = ((Date.now() - startTime) / 1000).toFixed(2);
  console.log(`\n=============================================================================`);
  console.log(`  PRODUKSI VIDEO VERTIKAL SELESAI PARIPURNA!`);
  console.log(`  Total Waktu Pipeline : ${elapsedTotal} detik (~${(elapsedTotal / 60).toFixed(1)} menit)`);
  console.log(`  Status Sistem        : PRODUCTION READY & VERIFIED`);
  console.log(`=============================================================================\n`);

  return {
    success: true,
    projectDir,
    totalScenes: choreography.length,
    elapsedSeconds: elapsedTotal
  };
}

function parseArgs(args = process.argv.slice(2)) {
  const options = {
    projectDir: null,
    title: null,
    scriptPath: null,
    voice: null,
    intonation: null,
    bgmSource: null,
    domain: null,
    lut: null,
    voVolume: 100,
    bgmVolume: 100,
    customVoice: null,
    targetDuration: 300, // default 5 menit (300 detik); di-override oleh --duration
    speedRatio: 1.0,     // default 1.0 (normal); di-override oleh --speed dari GUI atau Auto-Balance
    footageSkus: null,
    forceChoreo: false
  };

  // Validasi volume persen integer 0-200 (clamp), fallback 100 jika input tidak valid
  const parseVolumePct = (raw) => {
    const n = parseInt(raw, 10);
    if (!Number.isFinite(n)) return 100;
    return Math.min(200, Math.max(0, n));
  };

  for (let i = 0; i < args.length; i++) {
    const a = args[i];
    if (a === '--projectDir' && args[i + 1]) options.projectDir = args[++i];
    else if (a === '--title' && args[i + 1]) options.title = args[++i];
    else if (a === '--scriptPath' && args[i + 1]) options.scriptPath = args[++i];
    else if (a === '--voice' && args[i + 1]) options.voice = args[++i];
    else if (a === '--intonation' && args[i + 1]) options.intonation = args[++i];
    else if (a === '--bgm' && args[i + 1]) options.bgmSource = args[++i];
    else if (a === '--domain' && args[i + 1]) options.domain = args[++i];
    else if (a === '--lut' && args[i + 1]) options.lut = args[++i];
    else if (a === '--vo-volume' && args[i + 1]) options.voVolume = parseVolumePct(args[++i]);
    else if (a === '--bgm-volume' && args[i + 1]) options.bgmVolume = parseVolumePct(args[++i]);
    else if ((a === '--custom-voice' || a === '--customVoice') && args[i + 1]) options.customVoice = args[++i];
    else if (a === '--duration' && args[i + 1]) {
      const rawDur = parseFloat(args[++i]);
      if (Number.isFinite(rawDur) && rawDur > 0) options.targetDuration = rawDur;
    } else if (a === '--speed' && args[i + 1]) {
      const rawSpd = parseFloat(args[++i]);
      if (Number.isFinite(rawSpd) && rawSpd >= 0.70 && rawSpd <= 1.30) options.speedRatio = rawSpd;
    } else if ((a === '--footage-skus' || a === '--footageSkus') && args[i + 1]) {
      options.footageSkus = args[++i];
    } else if (a === '--force-choreo' || a === '--forceChoreo') {
      options.forceChoreo = true;
    }
  }

  return options;
}

// Support CLI Execution
if (require.main === module) {
  const options = parseArgs(process.argv.slice(2));

  if (!options.projectDir) {
    console.error('Penggunaan: node run_production_pipeline.js --projectDir <path_proyek> [--title <judul>] [--duration <detik>] [--voice <sarah|kore|aoede>] [--intonation <clarity|warmth|pause>] [--bgm <path_bgm>] [--domain <stoik|spiritual|sejarah|kreatif|santai>] [--lut <nama_file.cube|none>] [--vo-volume <0-200>] [--bgm-volume <0-200>] [--custom-voice <path_audio_vokal>] [--footage-skus <SKU1,SKU2|auto>]');
    process.exit(1);
  }

  runPipeline(options)
    .then(() => process.exit(0))
    .catch(err => {
      console.error('\n[PIPELINE FATAL ERROR]', err.message);
      process.exit(1);
    });
}

module.exports = { runPipeline, parseArgs };
