/**
 * generate_kinetic_subtitles.js — Generator Subtitle Kinetik ALL CAPS Emas
 * FokusKonten Video Engine — v2.0 Word-Level Snapping Edition
 *
 * Karakteristik:
 * - Font: Montserrat Black (bobot 900) 64pt
 * - MarginV: 580px (Center-Bottom Safe Zone untuk TikTok/Shorts/Reels)
 * - Outline 6.5px & Shadow 2.5px
 * - [v2.0] Word-Level Snapping: Waktu gelembung mengikuti stempel kata riil Faster-Whisper
 * - [v2.0] Fallback ke bagi-rata jika word_timing_master.json tidak tersedia
 * - [v2.0] Regex \b batas kata pada highlight emas — mencegah substring glitch
 * - Dynamic Highlighting: Aksen Emas ({\\c&H00D7FF&}) pada kata kunci emosional
 * - Gap Detector: Jika jeda antar kata > 0.35 detik, potong gelembung di sana (napas alami)
 */
'use strict';

const fs = require('fs');
const path = require('path');

// Format detik ke stempel waktu ASS (H:MM:SS.cs)
function toAssTime(sec) {
  const h = Math.floor(sec / 3600);
  const m = Math.floor((sec % 3600) / 60);
  const s = Math.floor(sec % 60);
  const cs = Math.floor((sec % 1) * 100);
  return `${h}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}.${cs.toString().padStart(2, '0')}`;
}

// Daftar kata kunci berbobot emosi tinggi untuk highlight emas
const DEFAULT_HIGHLIGHT_KEYWORDS = [
  'SLIP GAJI', 'NUMPANG LEWAT', 'KERJA KERASMU', 'MEMBAKAR TENAGA', 'MEMBAKAR REJEKIMU',
  'MEMATIKAN MIMPIMU', 'DISUAP SEDIKIT UANG', 'KERAJAAN BISNIS', 'PENGKHIANATAN TERBESAR',
  'KEAHLIAN BERNILAI TINGGI', 'ASET DIGITALMU', 'AMBIL KEMBALI KENDALI', 'TAKDIRMU',
  'PUTUSKAN NASIBMU', 'BUKAN PEMALAS', 'MEMPERALAT', 'MIMPIMU SENDIRI', 'LOYALITAS BUTA',
  'POTENSIMU', 'KEPUTUSAN', 'HARI INI', 'BANGKIT', 'SEKARANG JUGA', 'HABIS', 'KAPASITAS',
  'MENYELAMATKANNYA', 'MASA DEPAN', 'PUBLIK', 'GENGSI', 'HUKUM BESI', 'TUNGKU API',
  'SEJARAH', 'PEMENANG BESAR', 'DISIAPKAN', 'MENANG LEBIH BESAR',
  'DIAM', 'MENAKUTKAN', 'MARAH', 'WIBAWA', 'STOIKISME', 'KENDALI', 'BATASAN',
  'ENERGI', 'RUNTUH', 'KEKUATAN', 'TENANG', 'TATAPAN', 'PRINSIP', 'MASA DEPANMU',
  'TERLALU MAHAL', 'KETERAMPILAN', 'PEMENANG SEJATI', 'SERIGALA', 'SINGA',
  // [v4.0] Kata Kunci Stoikisme & Realitas Keras Eksklusif
  'HUKUM BESI', 'KETENANGAN MUTLAK', 'BADAI', 'PENDERITAAN', 'KEKEJAMAN NASIB', 'MEDAN PERANG',
  'AMARAH', 'KEKUASAAN', 'RAPUH', 'KETAKUTAN', 'MATAHARI', 'ANJING LIAR', 'BERAKAR KUAT',
  'KEBODOHANNYA', 'GELANGGANG', 'MUSUH TERBESARMU', 'PEDANG TAJAM', 'GEJOLAK EMOSIMU',
  'ORANG PICIK', 'LUMPUR', 'BINTANG', 'GONGGONGAN', 'HINAAN', 'DERAJATMU', 'PERTEMPURANMU',
  'KEPALA DINGIN', 'DARAH', 'KEABADIAN', 'KEHENINGANMU', 'BENTENG', 'RACUN', 'KEDENGKIAN',
  'MERUNTUHKAN', 'KEPEDIHAN', 'MELANGKAH TENANG', 'AKAL SEHAT', 'PEDANG PIKIRANMU',
  'ISTANA KEDAULATAN', 'SENYAP', 'TEPUK TANGAN', 'PENGAKUAN', 'KEBEBASAN SEJATIMU',
  'MELEDAK', 'MENGGETARKAN JAGAT', 'MISTERIUS', 'BAYANG-BAYANG', 'KEMULIAAN', 'RUANG PUBLIK',
  'PENGORBANAN BATIN', 'TEGAKKAN LANGKAHMU', 'NAKHODA MUTLAK', 'TAKDIRMU', 'BAJA', 'DITUNDUKKAN'
];

// Daftar Stopwords yang dihindari saat memilih kata penekanan fallback
const STOPWORDS = new Set([
  'DAN', 'DI', 'KE', 'DARI', 'YANG', 'INI', 'ITU', 'DENGAN', 'UNTUK', 'PADA', 'OLEH',
  'AGAR', 'SANGAT', 'AKAN', 'BISA', 'LAGI', 'JUGA', 'SAJA', 'ADALAH', 'SECARA', 'MAKA',
  'LALU', 'KARENA', 'SEBAB', 'KITA', 'KAMU', 'SAYA', 'MEREKA', 'DIA', 'KAU', 'TAK', 'TIDAK',
  'BUKAN', 'ADA', 'TIADA', 'SAAT', 'WAKTU', 'HANYA', 'JIKA', 'KALAU', 'SEPERTI', 'BAGAI'
]);

// [v3.0] Pemetaan Warna Aksen Kinetik Berbasis Semantik Naskah (Akurasi Rolling Tipografi)
function getHighlightColor(kw) {
  const upper = kw.toUpperCase();
  if (/AI|AGI|TEKNOLOGI|MODERN|CYBER|ROBOT|SISTEM/i.test(upper)) {
    return '&H00F0FF&'; // Neon Cyber Teal
  }
  if (/TITIK NOL|TERPURUK|PENDERITAAN|LINGKARAN SETAN|SAKIT|LUKA|KUTUKAN|KURUNGAN|TERJEBAK|MISKIN|GELAP/i.test(upper)) {
    return '&HFFFF00&'; // Cyan Dingin (Karakter Realitas Pahit)
  }
  return '&H00D7FF&'; // Emas Murni Klasik (Wibawa, Harapan, Anugerah)
}

/**
 * [v4.0] Terapkan highlight tematik dengan regex batas kata (\b)
 * Dilengkapi Smart Fallback Emphasis: Menjamin 100% gelembung memiliki 1 kata kuning emas
 */
function applyHighlightWithWordBoundary(chunkText, keywords) {
  let result = chunkText;
  let matched = false;

  for (const kw of keywords) {
    const escapedKw = kw.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const kwRegex = new RegExp(`(?<![A-Z])\\b(${escapedKw})\\b(?![A-Z])`, 'i');
    if (kwRegex.test(result)) {
      const color = getHighlightColor(kw);
      result = result.replace(
        kwRegex,
        `{\\c${color}}$1{\\c&HFFFFFF&}`
      );
      matched = true;
      break; // Satu highlight per gelembung cukup
    }
  }

  // [v4.0 Smart Fallback Emphasis] Jika tidak ada kata kunci yang cocok,
  // pilih 1 kata terpanjang non-stopword dalam gelembung untuk diberi highlight emas!
  if (!matched) {
    const rawTokens = chunkText.split(/\s+/);
    let bestWord = '';
    let maxScore = -999;

    for (const token of rawTokens) {
      const cleanToken = token.replace(/[^A-Za-z0-9]/g, '').toUpperCase();
      if (!cleanToken) continue;
      const isStop = STOPWORDS.has(cleanToken);
      const score = cleanToken.length - (isStop ? 5 : 0);
      if (score > maxScore) {
        maxScore = score;
        bestWord = token.replace(/[^A-Za-z0-9]/g, '');
      }
    }

    // Jika masih kosong (misal semua karakter aneh), ambil kata pertama
    if (!bestWord && rawTokens.length > 0) {
      bestWord = rawTokens[0].replace(/[^A-Za-z0-9]/g, '');
    }

    if (bestWord && bestWord.length >= 2) {
      const fallbackRegex = new RegExp(`\\b(${bestWord})\\b`, 'i');
      result = result.replace(fallbackRegex, `{\\c&H00D7FF&}$1{\\c&HFFFFFF&}`);
    }
  }

  return result;
}

/**
 * [v2.0] Bagi kata dalam satu kalimat (scene) ke gelembung dialog 3-kata
 * menggunakan stempel waktu KATA RIIL dari Faster-Whisper.
 * Jika jeda antar kata > GAP_THRESHOLD detik, potong gelembung di sana.
 */
const GAP_THRESHOLD = 0.85; // detik — hanya potong jika jeda hening sangat panjang

function buildBubblesFromWordTimings(scene, sceneWords, uniqueKeywords) {
  const events = [];
  if (!sceneWords || sceneWords.length === 0) return events;

  const WORDS_PER_BUBBLE = 3;
  const scriptWords = (scene.text || '').trim().replace(/^\uFEFF/, '').split(/\s+/).filter(Boolean);

  if (scriptWords.length > 0) {
    // Mode Penyelarasan Naskah Resmi ke Timing Kata Riil (Anti Typo/Salah Dengar Whisper)
    const numBubbles = Math.ceil(scriptWords.length / WORDS_PER_BUBBLE);

    for (let b = 0; b < numBubbles; b++) {
      const chunkWords = scriptWords.slice(b * WORDS_PER_BUBBLE, (b + 1) * WORDS_PER_BUBBLE);
      const fracStart = (b * WORDS_PER_BUBBLE) / scriptWords.length;
      const wIdxStart = Math.min(Math.floor(fracStart * sceneWords.length), sceneWords.length - 1);
      let bubbleStart = sceneWords[wIdxStart].start;

      // Snap ke awal adegan jika sangat dekat (<0.20s)
      if (b === 0 && (bubbleStart - scene.start) < 0.20 && (bubbleStart - scene.start) >= 0) {
        bubbleStart = scene.start;
      }

      let bubbleEnd;
      if (b < numBubbles - 1) {
        const fracNext = ((b + 1) * WORDS_PER_BUBBLE) / scriptWords.length;
        const wIdxNext = Math.min(Math.floor(fracNext * sceneWords.length), sceneWords.length - 1);
        bubbleEnd = sceneWords[wIdxNext].start;
      } else {
        // Gelembung terakhir: tutup tepat 0.35s setelah kata terakhir selesai diucapkan (anti-lingering saat jeda)
        const lastWordEnd = sceneWords[sceneWords.length - 1].end;
        bubbleEnd = Number(Math.min(lastWordEnd + 0.35, scene.end).toFixed(2));
      }

      if (bubbleEnd <= bubbleStart) {
        bubbleEnd = Number((bubbleStart + 0.60).toFixed(2));
      }

      let rawChunk = chunkWords.join(' ').toUpperCase();
      rawChunk = rawChunk.replace(/[{]/g, '(').replace(/[}]/g, ')').replace(/\\/g, '/');
      let chunkText = applyHighlightWithWordBoundary(rawChunk, uniqueKeywords);

      events.push(`Dialogue: 0,${toAssTime(bubbleStart)},${toAssTime(bubbleEnd)},Default,,0,0,0,,{\\fad(60,40)}${chunkText}`);
    }
  } else {

    // Fallback jika tidak ada naskah resmi: gunakan kata-kata dari Whisper
    const chunks = [];
    let i = 0;
    while (i < sceneWords.length) {
      const chunk = [];
      while (chunk.length < WORDS_PER_BUBBLE && i < sceneWords.length) {
        if (chunk.length > 0) {
          const prevWord = chunk[chunk.length - 1];
          const currWord = sceneWords[i];
          const gap = currWord.start - prevWord.end;
          if (gap > GAP_THRESHOLD) break;
        }
        chunk.push(sceneWords[i]);
        i++;
      }
      if (chunk.length > 0) {
        chunks.push(chunk);
      }
    }

    for (let c = 0; c < chunks.length; c++) {
      const chunk = chunks[c];
      let bubbleStart = chunk[0].start;
      if (c === 0 && (bubbleStart - scene.start) < 0.20 && (bubbleStart - scene.start) >= 0) {
        bubbleStart = scene.start;
      }

      let bubbleEnd;
      if (c < chunks.length - 1) {
        bubbleEnd = chunks[c + 1][0].start;
      } else {
        bubbleEnd = scene.end;
      }

      if (bubbleEnd <= bubbleStart) {
        bubbleEnd = Number((bubbleStart + 0.60).toFixed(2));
      }

      let rawChunk = chunk.map(w => w.word.toUpperCase()).join(' ');
      rawChunk = rawChunk.replace(/[{]/g, '(').replace(/[}]/g, ')').replace(/\\/g, '/');
      let chunkText = applyHighlightWithWordBoundary(rawChunk, uniqueKeywords);

      events.push(`Dialogue: 0,${toAssTime(bubbleStart)},${toAssTime(bubbleEnd)},Default,,0,0,0,,{\\fad(60,40)}${chunkText}`);
    }
  }

  return events;
}

/**
 * [v2.0 Fallback] Bagi rata matematis jika word_timing_master.json tidak tersedia
 */
function buildBubblesMathematical(scene, uniqueKeywords) {
  const events = [];
  const rawText = (scene.text || '').toUpperCase().trim();
  if (!rawText) return events;

  const words = rawText.split(/\s+/);
  if (words.length === 0) return events;

  const WORDS_PER_BUBBLE = 3;
  let bubbleCount = Math.ceil(words.length / WORDS_PER_BUBBLE);
  let bubbleDuration = scene.duration / bubbleCount;
  // Ambang batas anti-kedip (anti-strobe): jika durasi terlalu singkat (<0.75s), satukan dalam 1 gelembung tenang
  if (bubbleDuration < 0.75 && bubbleCount > 1) {
    bubbleCount = 1;
    bubbleDuration = scene.duration;
  }

  let bubbleStart = scene.start;

  for (let b = 0; b < bubbleCount; b++) {
    const startIdx = b * WORDS_PER_BUBBLE;
    const chunkWords = words.slice(startIdx, startIdx + WORDS_PER_BUBBLE);
    let chunkText = chunkWords.join(' ');

    chunkText = applyHighlightWithWordBoundary(chunkText, uniqueKeywords);

    const bubbleEnd = (b === bubbleCount - 1)
      ? scene.end
      : +(bubbleStart + bubbleDuration).toFixed(2);

    events.push(`Dialogue: 0,${toAssTime(bubbleStart)},${toAssTime(bubbleEnd)},Default,,0,0,0,,{\\fad(60,40)}${chunkText}`);
    bubbleStart = bubbleEnd;
  }

  return events;
}

/**
 * Menghasilkan konten string berkas ASS Studio Kinetik
 *
 * @param {object} options
 * @param {string} options.title - Judul video
 * @param {Array}  options.sceneTiming - Array timing per adegan kalimat
 * @param {Array}  [options.highlightKeywords] - Kata kunci tambahan untuk highlight emas
 * @param {Array}  [options.wordTimings] - Array kata dengan stempel waktu riil dari Faster-Whisper
 *                                         Jika ada, aktifkan Word-Level Snapping mode
 */
function generateKineticAss({ title = 'FokusKonten Kinetic Vertical', sceneTiming = [], highlightKeywords = [], wordTimings = null }) {
  const uniqueKeywords = [...new Set([...DEFAULT_HIGHLIGHT_KEYWORDS, ...highlightKeywords].map(k => k.toUpperCase()))]
    .sort((a, b) => b.length - a.length); // Urutan terpanjang dulu agar frasa lebih panjang menang

  const hasWordTimings = Array.isArray(wordTimings) && wordTimings.length > 0;
  if (hasWordTimings) {
    console.log(`[SUBTITLE] Mode: Word-Level Snapping Aktif (${wordTimings.length} kata riil dari Faster-Whisper)`);
  } else {
    console.log(`[SUBTITLE] Mode: Bagi-Rata Matematis (word_timing_master.json tidak tersedia)`);
  }

  const assHeader = `[Script Info]
Title: ${title}
ScriptType: v4.00+
WrapStyle: 0
ScaledBorderAndShadow: yes
YCbCr Matrix: TV.709
PlayResX: 1080
PlayResY: 1920

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Montserrat Black,64,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,-1,0,0,0,100,100,1,0,1,6.5,2.5,2,80,80,580,1
Style: HighlightYellow,Montserrat Black,64,&H0000D7FF,&H000000FF,&H00000000,&H80000000,-1,0,0,0,100,100,1,0,1,6.5,2.5,2,80,80,580,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
`;

  const allEvents = [];

  sceneTiming.forEach(scene => {
    if (!scene.text) return;

    if (hasWordTimings) {
      // Word-Level Snapping: aturan titik tengah kata (Word Midpoint Rule)
      // Menjamin setiap kata masuk ke tepat 1 adegan tanpa terbuang dan tanpa duplikasi
      const sceneWords = wordTimings.filter(w => {
        const mid = (w.start + w.end) / 2;
        const sStart = scene.speechStart !== undefined ? (scene.speechStart - 0.15) : (scene.start - 0.05);
        const sEnd   = scene.speechEnd !== undefined ? (scene.speechEnd + 0.15) : (scene.end + 0.05);
        return mid >= sStart && mid <= sEnd;
      });


      if (sceneWords.length > 0) {
        const bubbles = buildBubblesFromWordTimings(scene, sceneWords, uniqueKeywords);
        allEvents.push(...bubbles);
      } else {
        // Fallback jika tidak ada kata yang dipetakan ke scene ini
        const bubbles = buildBubblesMathematical(scene, uniqueKeywords);
        allEvents.push(...bubbles);
      }
    } else {
      // Mode bagi-rata jika tidak ada word timings
      const bubbles = buildBubblesMathematical(scene, uniqueKeywords);
      allEvents.push(...bubbles);
    }
  });

  return assHeader + allEvents.join('\n') + '\n';
}

/**
 * Membuat berkas ASS langsung ke path tujuan
 * Secara otomatis membaca word_timing_master.json dari direktori prompts jika tersedia
 */
function createKineticSubtitleFile(options, targetPath) {
  // Auto-detect word_timing_master.json dari direktori yang sama dengan targetPath
  let wordTimings = null;
  const targetDir = path.dirname(targetPath);

  // Cari di 04_prompts relatif terhadap project directory
  const possibleWordTimingPaths = [
    path.join(targetDir, 'word_timing_master.json'),
    path.join(targetDir, '..', '04_prompts', 'word_timing_master.json'),
    path.join(targetDir, '04_prompts', 'word_timing_master.json')
  ];

  for (const wtPath of possibleWordTimingPaths) {
    if (fs.existsSync(wtPath)) {
      try {
        wordTimings = JSON.parse(fs.readFileSync(wtPath, 'utf8'));
        console.log(`[SUBTITLE] word_timing_master.json ditemukan: ${wtPath} (${wordTimings.length} kata)`);
      } catch (e) {
        console.warn(`[SUBTITLE] Gagal membaca word_timing_master.json: ${e.message}`);
      }
      break;
    }
  }

  // Jika wordTimings tidak ditemukan via auto-detect, coba dari options.projectDir
  if (!wordTimings && options.projectDir) {
    const wtPath = path.join(options.projectDir, '04_prompts', 'word_timing_master.json');
    if (fs.existsSync(wtPath)) {
      try {
        wordTimings = JSON.parse(fs.readFileSync(wtPath, 'utf8'));
        console.log(`[SUBTITLE] word_timing_master.json ditemukan via projectDir: ${wtPath}`);
      } catch (e) {
        console.warn(`[SUBTITLE] Gagal membaca word_timing_master.json dari projectDir: ${e.message}`);
      }
    }
  }

  const content = generateKineticAss({ ...options, wordTimings });
  fs.writeFileSync(targetPath, content, 'utf8');
  console.log(`[SUBTITLE KINETIK] Berkas ASS berhasil dibuat: ${targetPath}`);
  return targetPath;
}

module.exports = {
  toAssTime,
  generateKineticAss,
  createKineticSubtitleFile,
  applyHighlightWithWordBoundary
};
