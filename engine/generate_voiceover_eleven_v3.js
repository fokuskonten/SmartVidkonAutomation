/**
 * generate_voiceover_eleven_v3.js
 * ══════════════════════════════════════════════════════════════════════════════
 * Generator Voiceover Headless 1-Klik FokusKonten Video Engine
 * Dual-Engine: Google Gemini TTS HD (Zero-Cost) + ElevenLabs v3
 * Fitur Sinkronisasi & Fallback Dua Arah (Gemini <-> ElevenLabs)
 * ══════════════════════════════════════════════════════════════════════════════
 */

const fs   = require('fs');
const path = require('path');
const { execSync } = require('child_process');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

const MODEL_ID_ELEVEN = 'eleven_v3';

// ── 1. SINKRONISASI VAULT SSOT (studio_api_vault.json) ─────────────────────
function getStudioApiVault() {
  const vaultPath = (pathsResolver && pathsResolver.getApiVaultPath)
    ? pathsResolver.getApiVaultPath()
    : path.join(__dirname, '..', 'config', 'studio_api_vault.json');
  if (fs.existsSync(vaultPath)) {
    try {
      const raw = fs.readFileSync(vaultPath, 'utf8').replace(/^\uFEFF/, '').trim();
      return JSON.parse(raw);
    } catch (e) {
      console.warn(`[API VAULT] Peringatan pembacaan vault: ${e.message}`);
    }
  }
  return null;
}

function getElevenKeys() {
  const vault = getStudioApiVault();
  const keys = [];
  if (vault && Array.isArray(vault.elevenlabs_keys)) {
    for (const item of vault.elevenlabs_keys) {
      if (item && item.is_active !== false && item.key) {
        keys.push(item.key.trim());
      }
    }
  }
  if (keys.length > 0) return Array.from(new Set(keys));
  if (process.env.ELEVEN_API_KEY) return [process.env.ELEVEN_API_KEY.trim()];
  return [];
}

// ── 2. KREDENSIAL SEKOCI GEMINI STUDIO ───────────────────────────────────────
function getGeminiKeys() {
  const vault = getStudioApiVault();
  const keys = [];
  if (vault && Array.isArray(vault.gemini_keys)) {
    for (const item of vault.gemini_keys) {
      if (item && item.is_active !== false && item.key) {
        keys.push(item.key.trim());
      }
    }
  }
  if (keys.length > 0) return Array.from(new Set(keys));
  if (process.env.GEMINI_API_KEY) return [process.env.GEMINI_API_KEY.trim()];
  return [];
}

// ── 3. REGISTRI KARAKTER SUARA ───────────────────────────────────────────────
const GEMINI_VOICES = {
  kore:   { name: 'Kore',   desc: 'Wanita Tenang & Berkelas', gender: 'female' },
  aoede:  { name: 'Aoede',  desc: 'Wanita Hangat & Ekspresif', gender: 'female' },
  leda:   { name: 'Leda',   desc: 'Wanita Muda & Ringan', gender: 'female' },
  zephyr: { name: 'Zephyr', desc: 'Wanita Lembut & Natural', gender: 'female' },
  charon: { name: 'Charon', desc: 'Pria Berwibawa & Dokumenter', gender: 'male' },
  fenrir: { name: 'Fenrir', desc: 'Pria Tegas & Teknis', gender: 'male' },
  puck:   { name: 'Puck',   desc: 'Pria Ceria & Energik', gender: 'male' },
  orus:   { name: 'Orus',   desc: 'Pria Netral & Bersih', gender: 'male' }
};

const ELEVEN_VOICES = {
  sarah:   { id: 'EXAVITQu4vr4xnSDxMaL', name: 'Sarah (Wanita Bijak & Mengalir)', gender: 'female' },
  arunika: { id: 'cgSgspJ2msm6clMCkdW9', name: 'Arunika (Audio Master Studio)', gender: 'female' },
  george:  { id: 'JBFqnCBsd6RMkjVDRZzb', name: 'George (Pria Narator Stoik)', gender: 'male' },
  roger:   { id: 'CwhRBWXzGAHq8TQ4Fs17', name: 'Roger (Pria Resonan & Maskulin)', gender: 'male' },
  alice:   { id: 'Xb7hH8MSUJpSbSDYk0k2', name: 'Alice (Wanita Edukatif Jernih)', gender: 'female' }
};

// ── 4. PRESET INTONASI ELEVENLABS ────────────────────────────────────────────
// Dikalibrasi presisi untuk retensi penonton tinggi: kestabilan, ekspresif, dan jeda mikro
const INTONATION_PRESETS = {
  clarity:     { stability: 0.42, similarity_boost: 0.90, style: 0.35, use_speaker_boost: true }, // Tegas, Wibawa & Heroik
  pause:       { stability: 0.50, similarity_boost: 0.85, style: 0.22, use_speaker_boost: true }, // Tenang, Dingin & Stoik
  stoic:       { stability: 0.50, similarity_boost: 0.85, style: 0.22, use_speaker_boost: true }, // Stoikisme Reflektif
  dark:        { stability: 0.52, similarity_boost: 0.84, style: 0.28, use_speaker_boost: true }, // Misterius & Dark Mind
  warmth:      { stability: 0.36, similarity_boost: 0.82, style: 0.34, use_speaker_boost: true }, // Hangat & Empatik Storytelling
  documentary: { stability: 0.55, similarity_boost: 0.88, style: 0.16, use_speaker_boost: true }, // Dokumenter Lugas & Realitas Keras
  spiritual:   { stability: 0.48, similarity_boost: 0.86, style: 0.24, use_speaker_boost: true }  // Spiritual & Khidmat
};

// Helper: Bungkus PCM Buffer ke Header WAV 44 Byte
function pcmToWavBuffer(pcmData, sampleRate = 24000, numChannels = 1, bitsPerSample = 16) {
  const byteRate   = sampleRate * numChannels * (bitsPerSample / 8);
  const blockAlign = numChannels * (bitsPerSample / 8);
  const dataSize   = pcmData.length;
  const chunkSize  = 36 + dataSize;

  const header = Buffer.alloc(44);
  header.write('RIFF', 0);
  header.writeUInt32LE(chunkSize, 4);
  header.write('WAVE', 8);
  header.write('fmt ', 12);
  header.writeUInt32LE(16, 16);
  header.writeUInt16LE(1, 20);
  header.writeUInt16LE(numChannels, 22);
  header.writeUInt32LE(sampleRate, 24);
  header.writeUInt32LE(byteRate, 28);
  header.writeUInt16LE(blockAlign, 32);
  header.writeUInt16LE(bitsPerSample, 34);
  header.write('data', 36);
  header.writeUInt32LE(dataSize, 40);

  return Buffer.concat([header, pcmData]);
}

// ── 5. GENERATOR GEMINI TTS NATIVE ───────────────────────────────────────────
async function synthesizeWithGeminiTts(inputText, geminiVoiceName = 'Kore', outputMp3Path) {
  const keys = getGeminiKeys();
  if (!keys || keys.length === 0) {
    throw new Error('[TTS ERROR] Kunci API Gemini belum dikonfigurasi di Smart Vidkon Automation. Buka menu "API Key Vault" untuk menambahkan Kunci API Gemini Anda (Gratis dari Google AI Studio).');
  }
  const selectedVoice = GEMINI_VOICES[geminiVoiceName.toLowerCase()] ? GEMINI_VOICES[geminiVoiceName.toLowerCase()].name : 'Kore';

  console.log(`[Gemini TTS HD] Memulai generasi vokal (Suara: ${selectedVoice}, Kecepatan: Normal Alami)...`);
  console.log(`[Gemini TTS HD] Panjang: ${inputText.split(/\s+/).filter(Boolean).length} kata (${inputText.length} karakter)`);

  const payload = JSON.stringify({
    contents: [{ parts: [{ text: inputText }] }],
    generationConfig: {
      responseModalities: ['AUDIO'],
      speechConfig: {
        voiceConfig: {
          prebuiltVoiceConfig: { voiceName: selectedVoice }
        }
      }
    }
  });



  for (let i = 0; i < keys.length; i++) {
    const key = keys[i];
    const keyLabel = `SEKOCI GEMINI #${i + 1} (...${key.slice(-6)})`;
    try {
      const url = `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-tts:generateContent?key=${key}`;
      const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: payload
      });

      if (res.status === 429 || res.status === 403) {
        console.warn(`[Gemini TTS HD] ${keyLabel} HTTP ${res.status} — rotasi sekoci...`);
        continue;
      }

      if (!res.ok) {
        const errText = await res.text();
        console.warn(`[Gemini TTS HD] ${keyLabel} HTTP ${res.status}: ${errText.substring(0, 100)}`);
        continue;
      }

      const data = await res.json();
      const audioB64 = data?.candidates?.[0]?.content?.parts?.[0]?.inlineData?.data;
      if (!audioB64) continue;

      const rawPcm = Buffer.from(audioB64, 'base64');
      const wavBuffer = pcmToWavBuffer(rawPcm, 24000, 1, 16);

      // Simpan sementara file WAV lalu konversi ke MP3 via FFmpeg agar konsisten
      const tempWav = outputMp3Path.replace(/\.mp3$/i, '_temp.wav');
      fs.mkdirSync(path.dirname(tempWav), { recursive: true });
      fs.writeFileSync(tempWav, wavBuffer);

      try {
        execSync(`ffmpeg -y -i "${tempWav}" -codec:a libmp3lame -b:a 320k "${outputMp3Path}"`, { stdio: 'pipe' });
        try { fs.unlinkSync(tempWav); } catch (_) {}
      } catch (errFfmpeg) {
        // Fallback jika ffmpeg gagal: rename wav ke path tujuan
        fs.renameSync(tempWav, outputMp3Path);
      }

      const stat = fs.statSync(outputMp3Path);
      console.log(`[Gemini TTS HD] SUKSES via ${keyLabel}!`);
      console.log(`[Gemini TTS HD] Berkas tersimpan: ${outputMp3Path} (${(stat.size / 1024).toFixed(1)} KB)`);
      return outputMp3Path;
    } catch (e) {
      console.warn(`[Gemini TTS HD] Error pada ${keyLabel}: ${e.message}`);
    }
  }

  throw new Error('Semua sekoci Gemini TTS gagal merespons.');
}

// ── 6. GENERATOR ELEVENLABS TTS ──────────────────────────────────────────────
async function synthesizeWithElevenLabs(inputText, elevenVoiceKey = 'sarah', mode = 'clarity', outputMp3Path, speedRatio = 1.0) {
  const cleanKey = elevenVoiceKey.toLowerCase();
  const voiceObj = ELEVEN_VOICES[cleanKey] || ELEVEN_VOICES.sarah;
  const activeVoiceId = voiceObj.id;
  const activeVoiceName = voiceObj.name;
  const voiceSettings = INTONATION_PRESETS[mode] || INTONATION_PRESETS.clarity;

  // Clamp speedRatio ke rentang ElevenLabs yang aman
  const finalSpeed = Math.max(0.75, Math.min(1.25, speedRatio || 1.0));
  const speedLabel = Math.abs(finalSpeed - 1.0) > 0.01 ? ` | Kecepatan: ${finalSpeed.toFixed(2)}x` : '';

  console.log(`[Eleven v3 TTS] Memulai proses generate narasi vokal...`);
  console.log(`[Eleven v3 TTS] Panjang karakter: ${inputText.length}${speedLabel}`);
  console.log(`[Eleven v3 TTS] Karakter Suara  : ${activeVoiceName} (${activeVoiceId})`);

  // Bangun payload: speed hanya dikirim jika bukan 1.0
  const buildPayload = () => {
    const p = {
      text: inputText,
      model_id: MODEL_ID_ELEVEN,
      language_code: 'id',
      voice_settings: voiceSettings
    };
    if (Math.abs(finalSpeed - 1.0) > 0.01) p.speed = finalSpeed;
    return JSON.stringify(p);
  };

  let response = null;
  let lastError = null;

  const elevenKeys = getElevenKeys();
  if (!elevenKeys || elevenKeys.length === 0) {
    throw new Error('[TTS ERROR] Kunci API ElevenLabs belum dikonfigurasi di Smart Vidkon Automation. Buka menu "API Key Vault" untuk menambahkan Kunci API ElevenLabs Anda.');
  }
  for (let i = 0; i < elevenKeys.length; i++) {
    const currentKey = elevenKeys[i];
    const keyLabel = `SEKOCI #${i + 1} (${currentKey.slice(0, 8)}...${currentKey.slice(-4)})`;
    try {
      response = await fetch(`https://api.elevenlabs.io/v1/text-to-speech/${activeVoiceId}`, {
        method: 'POST',
        headers: {
          'xi-api-key': currentKey,
          'Content-Type': 'application/json'
        },
        body: buildPayload()
      });

      if (response.ok) {
        console.log(`[Eleven v3 TTS] Panggilan API berhasil via ${keyLabel}!`);
        break;
      } else {
        const errText = await response.text();
        lastError = `HTTP ${response.status}: ${errText}`;
        console.warn(`[Eleven v3 TTS] Peringatan: ${keyLabel} gagal (${lastError}). Mencoba failover...`);
      }
    } catch (err) {
      lastError = err.message;
      console.warn(`[Eleven v3 TTS] Network error pada ${keyLabel}: ${err.message}. Mencoba failover...`);
    }
  }

  if (!response || !response.ok) {
    throw new Error(`Seluruh API Key ElevenLabs gagal digunakan. Error terakhir: ${lastError}`);
  }

  const audioBuffer = Buffer.from(await response.arrayBuffer());
  fs.mkdirSync(path.dirname(outputMp3Path), { recursive: true });
  fs.writeFileSync(outputMp3Path, audioBuffer);

  const stat = fs.statSync(outputMp3Path);
  console.log(`[Eleven v3 TTS] SUKSES! File tersimpan: ${outputMp3Path} (${(stat.size / 1024).toFixed(1)} KB)`);
  return outputMp3Path;
}

// ── 7. MASTER FUNCTION: SINKRONISASI & DUAL-ENGINE FALLBACK ──────────────────
async function generateVoiceoverSingle({ text, outputFilePath, mode = 'clarity', voice = 'kore', speedRatio = 1.0 }) {
  const normalizedVoice = (voice || 'kore').toLowerCase().replace('gemini_', '');
  const isGeminiVoice = Boolean(GEMINI_VOICES[normalizedVoice]);
  const isElevenVoice = Boolean(ELEVEN_VOICES[normalizedVoice]);

  if (isGeminiVoice || normalizedVoice === 'auto' || normalizedVoice === 'kore') {
    const chosenGeminiVoice = GEMINI_VOICES[normalizedVoice] ? normalizedVoice : 'kore';
    try {
      return await synthesizeWithGeminiTts(text, chosenGeminiVoice, outputFilePath);
    } catch (errGemini) {
      console.warn(`\n[FALLBACK AKTIF] Gemini TTS mengalami gangguan: ${errGemini.message}`);
      console.warn(`[FALLBACK AKTIF] Otomatis beralih ke ElevenLabs (Sarah)...`);
      return await synthesizeWithElevenLabs(text, 'sarah', mode, outputFilePath, speedRatio);
    }
  } else if (isElevenVoice) {
    try {
      return await synthesizeWithElevenLabs(text, normalizedVoice, mode, outputFilePath, speedRatio);
    } catch (errEleven) {
      console.warn(`\n[FALLBACK AKTIF] Kuota/Koneksi ElevenLabs gagal: ${errEleven.message}`);
      console.warn(`[FALLBACK AKTIF] Otomatis beralih ke Gemini HD (Kore)...`);
      return await synthesizeWithGeminiTts(text, 'Kore', outputFilePath);
    }
  } else {
    try {
      return await synthesizeWithGeminiTts(text, 'Kore', outputFilePath);
    } catch (_) {
      return await synthesizeWithElevenLabs(text, 'sarah', mode, outputFilePath, speedRatio);
    }
  }
}

async function generateVoiceover({ text, textFilePath, outputFilePath, mode = 'clarity', voice = 'kore', customSettings, speedRatio = 1.0 }) {
  let inputText = text;
  if (!inputText && textFilePath) {
    if (!fs.existsSync(textFilePath)) {
      throw new Error(`File teks naskah tidak ditemukan: ${textFilePath}`);
    }
    inputText = fs.readFileSync(textFilePath, 'utf8').trim();
  }

  if (!inputText) {
    throw new Error('Naskah teks wajib disertakan via argumen text atau textFilePath.');
  }

  const finalOutputPath = outputFilePath || path.join(__dirname, 'output_voiceover.mp3');

  // ── AUTO-CHUNKING HANYA JIKA MELEBIHI 2400 KATA (> 2400 KATA) ───────────────
  // Naskah sampai 2.400 kata (termasuk 1.126 kata ~10 menit) diproses UTUH 1 berkas normal tanpa dibagi
  const totalWords = inputText.split(/\s+/).filter(Boolean).length;
  const MAX_CHUNK_WORDS = 2400;

  if (totalWords > MAX_CHUNK_WORDS) {
    console.log(`\n[AUTO-CHUNKING TTS] Naskah sangat panjang terdeteksi (${totalWords} kata / ${inputText.length} karakter).`);
    console.log(`[AUTO-CHUNKING TTS] Membagi ke dalam segmen per ~${MAX_CHUNK_WORDS} kata agar audio master tetap stabil...`);

    const lines = inputText.split('\n').map(l => l.trim()).filter(Boolean);
    const chunks = [];
    let currentChunk = [];
    let currentWords = 0;

    for (const line of lines) {
      const lineWords = line.split(/\s+/).filter(Boolean).length;
      if (currentWords + lineWords > MAX_CHUNK_WORDS && currentChunk.length > 0) {
        chunks.push(currentChunk.join('\n'));
        currentChunk = [line];
        currentWords = lineWords;
      } else {
        currentChunk.push(line);
        currentWords += lineWords;
      }
    }
    if (currentChunk.length > 0) chunks.push(currentChunk.join('\n'));

    console.log(`[AUTO-CHUNKING TTS] Terbagi menjadi ${chunks.length} bagian babak.`);
    const tempDir = path.join(path.dirname(finalOutputPath), '_temp_chunks_' + Date.now());
    fs.mkdirSync(tempDir, { recursive: true });
    const chunkFiles = [];

    for (let c = 0; c < chunks.length; c++) {
      const chunkText = chunks[c];
      const chunkOut = path.join(tempDir, `chunk_${String(c + 1).padStart(3, '0')}.mp3`);
      console.log(`\n  --- Memproses Babak [${c + 1}/${chunks.length}] (${chunkText.split(/\s+/).filter(Boolean).length} kata) ---`);
      await generateVoiceoverSingle({
        text: chunkText,
        outputFilePath: chunkOut,
        mode,
        voice,
        speedRatio
      });
      chunkFiles.push(chunkOut);
    }

    // Gabungkan seluruh babak dengan FFmpeg concat list
    console.log(`\n[AUTO-CHUNKING TTS] Menggabungkan ${chunkFiles.length} babak audio secara seamless...`);
    const concatList = path.join(tempDir, 'concat_list.txt');
    const concatContent = chunkFiles.map(f => `file '${f.replace(/\\/g, '/')}'`).join('\n');
    fs.writeFileSync(concatList, concatContent, 'utf8');

    execSync(`ffmpeg -y -f concat -safe 0 -i "${concatList}" -c:a libmp3lame -b:a 320k "${finalOutputPath}"`, { stdio: 'pipe' });


    try {
      fs.rmSync(tempDir, { recursive: true, force: true });
    } catch (_) {}

    const stat = fs.statSync(finalOutputPath);
    console.log(`[AUTO-CHUNKING TTS] SUKSES PARIPURNA! Berkas utuh: ${finalOutputPath} (${(stat.size / (1024 * 1024)).toFixed(2)} MB)\n`);
    return finalOutputPath;
  }

  // Naskah berukuran normal (<= 2400 kata): proses langsung
  return await generateVoiceoverSingle({ text: inputText, outputFilePath: finalOutputPath, mode, voice, speedRatio });
}


// Support CLI execution: node generate_voiceover_eleven_v3.js <inputFile.txt> [outputFile.mp3] [mode] [voice] [--speed 0.92]
if (require.main === module) {
  const args = process.argv.slice(2);

  // Parsing --speed dari argv (format: --speed 0.92)
  let speedRatio = 1.0;
  const speedIdx = args.indexOf('--speed');
  if (speedIdx !== -1 && args[speedIdx + 1]) {
    const parsed = parseFloat(args[speedIdx + 1]);
    if (!isNaN(parsed) && parsed > 0.5 && parsed < 2.0) {
      speedRatio = Math.max(0.75, Math.min(1.25, parsed));
    }
  }

  // Argumen posisional (hapus --speed dan nilainya sebelum parsing posisional)
  const cleanArgs = args.filter((a, i) => a !== '--speed' && args[i - 1] !== '--speed');
  const inputArg  = cleanArgs[0];
  const outputArg = cleanArgs[1];
  const modeArg   = cleanArgs[2] || 'clarity';
  const voiceArg  = cleanArgs[3] || 'kore';

  if (!inputArg) {
    console.log('Penggunaan CLI: node generate_voiceover_eleven_v3.js <path_teks.txt> [path_output.mp3] [mode] [voice] [--speed 0.92]');
    process.exit(1);
  }

  if (Math.abs(speedRatio - 1.0) > 0.01) {
    console.log(`[KECEPATAN TTS] Kecepatan vokal disetel: ${speedRatio.toFixed(2)}x (dari Auto-Balance atau slider manual)`);
  }

  const isFile = fs.existsSync(inputArg);
  const options = isFile
    ? { textFilePath: inputArg, outputFilePath: outputArg, mode: modeArg, voice: voiceArg, speedRatio }
    : { text: inputArg, outputFilePath: outputArg, mode: modeArg, voice: voiceArg, speedRatio };

  generateVoiceover(options)
    .then(() => process.exit(0))
    .catch(err => {
      console.error(`[VOICEOVER ERROR]`, err.message);
      process.exit(1);
    });
}

module.exports = {
  generateVoiceover,
  INTONATION_PRESETS,
  GEMINI_VOICES,
  ELEVEN_VOICES
};
