/**
 * paths_resolver.js — Modul Resolusi Jalur Relatif & Isolasi Mandiri Smart Vidkon Automation
 * ══════════════════════════════════════════════════════════════════════════════
 * Menyediakan pemetaan jalur portabel terisolasi 100% berbasis direktori aplikasi
 * tanpa kebergantungan pada jalur hardcoded lingkungan pengembang.
 * ══════════════════════════════════════════════════════════════════════════════
 */
'use strict';

const fs = require('fs');
const path = require('path');

// Direktori Utama Aplikasi Portabel
const ROOT_DIR = path.resolve(__dirname, '..');
const BIN_DIR = path.join(ROOT_DIR, 'bin');
const CONFIG_DIR = path.join(ROOT_DIR, 'config');
const ASSETS_DIR = path.join(ROOT_DIR, 'assets');
const DATABASE_DIR = path.join(ROOT_DIR, 'database');
const PROJECTS_DIR = path.join(ROOT_DIR, 'projects');
const BGM_DIR = path.join(ASSETS_DIR, 'bgm');
const LUTS_DIR = path.join(ASSETS_DIR, 'luts');
const BRANDING_DIR = path.join(ASSETS_DIR, 'branding');
const DESKTOP_DIR = path.join(ROOT_DIR, 'desktop');
const DOCS_DIR = path.join(ROOT_DIR, 'docs');

/**
 * Resolusi jalur database SQLite footage_catalog.db
 */
function getDatabasePath() {
  return path.join(DATABASE_DIR, 'footage_catalog.db');
}

/**
 * Resolusi ringkasan katalog JSON
 */
function getCatalogSummaryPath() {
  return path.join(DATABASE_DIR, 'katalog_vidcon_summary.json');
}

/**
 * Resolusi katalog lokal JSON lengkap
 */
function getCatalogLokalPath() {
  return path.join(DATABASE_DIR, 'katalog_vidcon_lokal.json');
}

/**
 * Resolusi vault API (Gemini, ElevenLabs, Pixabay)
 */
function getApiVaultPath() {
  return path.join(CONFIG_DIR, 'studio_api_vault.json');
}

/**
 * Resolusi media vault JSON
 */
function getMediaVaultPath() {
  return path.join(CONFIG_DIR, 'studio_media_vault.json');
}

/**
 * Resolusi app_config.json
 */
function getAppConfigPath() {
  return path.join(CONFIG_DIR, 'app_config.json');
}

/**
 * Membaca konfigurasi aplikasi
 */
function loadAppConfig() {
  const p = getAppConfigPath();
  if (fs.existsSync(p)) {
    try {
      return JSON.parse(fs.readFileSync(p, 'utf8'));
    } catch (e) {
      console.warn(`[PATHS RESOLVER] Gagal membaca app_config.json: ${e.message}`);
    }
  }
  return {
    appName: 'Smart Vidkon Automation',
    version: '1.0.0',
    port: 8092,
    hardwareAcceleration: { nvenc: true }
  };
}

/**
 * Resolusi direktori pustaka BGM
 */
function getBgmLibraryDir() {
  return BGM_DIR;
}

/**
 * Resolusi direktori LUTs
 */
function getLutsDir() {
  return LUTS_DIR;
}

/**
 * Resolusi direktori branding
 */
function getBrandingDir() {
  return BRANDING_DIR;
}

/**
 * Resolusi video outro bumper 3D FokusKonten
 */
function getLogoOutroPath() {
  return path.join(BRANDING_DIR, 'Logo_on_obsidian_pedestal_1080p_20260929231658.mp4');
}

/**
 * Resolusi berkas biner portable (bin/node.exe, bin/ffmpeg.exe, bin/ffprobe.exe)
 */
function getBinaryPath(binaryName) {
  const exeName = binaryName.endsWith('.exe') ? binaryName : `${binaryName}.exe`;
  const localBin = path.join(BIN_DIR, exeName);
  if (fs.existsSync(localBin)) return localBin;
  return binaryName; // Fallback ke sistem PATH jika biner tidak dibundel
}

/**
 * Mengambil daftar folder aktif yang didaftarkan pengguna di studio_media_vault.json
 */
function getActiveMediaVaultFolders(type = 'Footage') {
  const vaultPath = getMediaVaultPath();
  if (fs.existsSync(vaultPath)) {
    try {
      const data = JSON.parse(fs.readFileSync(vaultPath, 'utf8'));
      if (data && Array.isArray(data.folders)) {
        const result = [];
        for (const f of data.folders) {
          if (!f || !f.is_active || !f.folder_path) continue;
          if (type) {
            if (type === 'Footage') {
              const isFootage = f.type === 'Footage' || f.type === 'FootageVertical' || f.type === 'FootageVertikal' || f.type === 'FootageLandscape';
              if (!isFootage) continue;
            } else if (f.type !== type) {
              continue;
            }
          }

          let targetPath = f.folder_path;
          if (targetPath.startsWith('./') || targetPath.startsWith('.\\')) {
            targetPath = path.resolve(ROOT_DIR, targetPath);
          }

          if (fs.existsSync(targetPath)) {
            result.push(targetPath);
          }
        }
        return result;
      }
    } catch (e) {}
  }
  return [];
}

/**
 * Resolusi jalur file video dari record database (menangani branding lokal dan folder footage kustom pengguna)
 */
function resolveVideoPath(videoPath) {
  if (!videoPath) return videoPath;
  if (fs.existsSync(videoPath)) return videoPath;

  const baseName = path.basename(videoPath);

  // 1. Periksa apakah ini aset branding
  const brandingCandidate = path.join(BRANDING_DIR, baseName);
  if (fs.existsSync(brandingCandidate)) return brandingCandidate;

  // 2. Periksa seluruh folder footage yang didaftarkan pengguna secara aktif di media vault
  const activeFolders = getActiveMediaVaultFolders('Footage');
  for (const fld of activeFolders) {
    // Cek direct file di root folder footage
    const candidateDirect = path.join(fld, baseName);
    if (fs.existsSync(candidateDirect)) return candidateDirect;

    // Cek jika klip berada di dalam subfolder bundel (misal: ID1646_CYBERPUNK/clip_01.mp4)
    if (videoPath.includes('\\') || videoPath.includes('/')) {
      const parts = videoPath.split(/[\\\/]+/);
      if (parts.length >= 2) {
        const subRel = path.join(parts[parts.length - 2], baseName);
        const candidateSub = path.join(fld, subRel);
        if (fs.existsSync(candidateSub)) return candidateSub;
      }
    }
  }

  return videoPath;
}

/**
 * Resolusi jalur berkas snapshot frame
 */
function resolveSnapshotPath(snapshotPath) {
  if (!snapshotPath) return snapshotPath;
  if (fs.existsSync(snapshotPath)) return snapshotPath;

  const baseName = path.basename(snapshotPath);
  const localSnap = path.join(DATABASE_DIR, '_snapshots', baseName);
  if (fs.existsSync(localSnap)) return localSnap;

  return snapshotPath;
}

module.exports = {
  ROOT_DIR,
  BIN_DIR,
  CONFIG_DIR,
  ASSETS_DIR,
  DATABASE_DIR,
  PROJECTS_DIR,
  BGM_DIR,
  LUTS_DIR,
  BRANDING_DIR,
  DESKTOP_DIR,
  DOCS_DIR,
  getDatabasePath,
  getCatalogSummaryPath,
  getCatalogLokalPath,
  getApiVaultPath,
  getMediaVaultPath,
  getAppConfigPath,
  loadAppConfig,
  getBgmLibraryDir,
  getLutsDir,
  getBrandingDir,
  getLogoOutroPath,
  getBinaryPath,
  getActiveMediaVaultFolders,
  resolveVideoPath,
  resolveSnapshotPath
};
