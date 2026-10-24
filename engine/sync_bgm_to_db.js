/**
 * sync_bgm_to_db.js — Helper Sinkronisasi Cerdas BGM Baru ke SQLite soundtrack.db
 * Zero-Cost, Pure Node.js SQLite built-in + ffprobe ground-truth audio timing
 */
'use strict';

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const { DatabaseSync } = require('node:sqlite');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

const SOUNDTRACK_DB_PATH = (pathsResolver && pathsResolver.DATABASE_DIR && fs.existsSync(path.join(pathsResolver.DATABASE_DIR, 'soundtrack.db')))
  ? path.join(pathsResolver.DATABASE_DIR, 'soundtrack.db')
  : path.join(__dirname, '..', 'database', 'soundtrack.db');

/**
 * Ekstrak durasi ground-truth via ffprobe
 */
function getAudioDuration(filePath) {
  try {
    const out = execSync(`ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "${filePath}"`, {
      timeout: 4000,
      stdio: ['ignore', 'pipe', 'ignore']
    }).toString().trim();
    const dur = parseFloat(out);
    return !isNaN(dur) && dur > 0 ? Math.round(dur * 10) / 10 : 180.0;
  } catch (e) {
    return 180.0;
  }
}

/**
 * Deteksi genre dan mood otomatis dari judul track
 */
function detectGenreAndMood(fileName) {
  const lower = fileName.toLowerCase();
  let genre = 'Cinematic';
  let mood = 'Reflective';

  if (/obito|death|despair|dark|joker|tense|bad_style/.test(lower)) {
    genre = 'Dark Cinematic';
    mood = 'Dark & Tension';
  } else if (/sadness|sorrow|melancholy|tersedih/.test(lower)) {
    genre = 'Melancholic Cinematic';
    mood = 'Deep Emotional';
  } else if (/peace|daytime|inner|shirfine/.test(lower)) {
    genre = 'Ambient Calm';
    mood = 'Peaceful & Reflective';
  } else if (/violin|cello|orchestral|piano|trailer|dramatic/.test(lower)) {
    genre = 'Orchestral Instrumental';
    mood = 'Dramatic & Inspiring';
  } else if (/trap|remix/.test(lower)) {
    genre = 'Modern Beats';
    mood = 'Energetic Focus';
  }

  return { genre, mood };
}

function syncBgmTrack(fileName, fullPath) {
  if (!fs.existsSync(SOUNDTRACK_DB_PATH)) {
    console.log('[SYNC] soundtrack.db tidak ditemukan, melewati sinkronisasi SQLite.');
    return;
  }

  const db = new DatabaseSync(SOUNDTRACK_DB_PATH);
  try { db.exec('PRAGMA journal_mode = WAL;'); db.exec('PRAGMA busy_timeout = 5000;'); } catch (e) {}
  try {
    const stat = fs.existsSync(fullPath) ? fs.statSync(fullPath) : { size: 0 };
    const baseName = path.basename(fileName, path.extname(fileName));
    const title = baseName.replace(/^\d+_/, '').replace(/_/g, ' ');
    const durationSec = getAudioDuration(fullPath);
    const { genre, mood } = detectGenreAndMood(fileName);

    // Cek apakah sudah terdaftar
    const existing = db.prepare('SELECT id, track_sku FROM audio_tracks WHERE cdn_filename = ? OR title = ?').get(fileName, title);
    if (existing) {
      db.prepare(`
        UPDATE audio_tracks 
        SET duration_sec = ?, file_size_bytes = ?, genre = ?, mood = ?
        WHERE id = ?
      `).run(durationSec, stat.size, genre, mood, existing.id);

      console.log(`[BGM DB SYNC] Track '${title}' (${existing.track_sku}) diperbarui: Durasi ${durationSec}s | Mood: ${mood} | Ukuran: ${(stat.size / (1024*1024)).toFixed(2)} MB`);
      return;
    }

    // Hitung nomor urut SKU baru
    const countRow = db.prepare("SELECT COUNT(*) as cnt FROM audio_tracks WHERE source_volume = 'STUDIO'").get();
    const nextNum = (countRow ? countRow.cnt : 0) + 1;
    const sku = `EDU-BGM-${String(nextNum).padStart(4, '0')}`;

    const insertStmt = db.prepare(`
      INSERT INTO audio_tracks (
        track_sku, source_volume, original_folder, title,
        duration_sec, file_size_bytes, bpm, musical_key,
        genre, mood, cdn_shard, cdn_filename, cdn_url, safelink_url, is_published
      ) VALUES (
        ?, 'STUDIO', '00_SSOT', ?,
        ?, ?, 120, 'C',
        ?, ?, 'studio', ?, 'local', 'local', 1
      )
    `);

    insertStmt.run(sku, title, durationSec, stat.size, genre, mood, fileName);

    // Cek dan perbarui FTS jika virtual table ada
    try {
      const ftsCheck = db.prepare("SELECT name FROM sqlite_master WHERE type='table' AND name='audio_tracks_fts'").get();
      if (ftsCheck) {
        db.prepare("INSERT INTO audio_tracks_fts (rowid, title, genre, mood) VALUES (last_insert_rowid(), ?, ?, ?)").run(title, genre, mood);
      }
    } catch (e) {}

    console.log(`[BGM DB SYNC] BERHASIL DAFTAR: '${title}' (${sku}) | Durasi: ${durationSec}s | Mood: ${mood} | ${(stat.size / (1024*1024)).toFixed(2)} MB`);
  } catch (err) {
    console.error(`[BGM DB SYNC ERROR] ${err.message}`);
  } finally {
    db.close();
  }
}

// CLI runner
const args = process.argv.slice(2);
if (args.length >= 2) {
  syncBgmTrack(args[0], args[1]);
} else if (args.length === 1) {
  syncBgmTrack(path.basename(args[0]), args[0]);
}

module.exports = { syncBgmTrack };
