/**
 * fetch_stock_videos_pixabay.js
 * ══════════════════════════════════════════════════════════════════════════════
 * Pengunduh Otomatis Stock Footage Video Pixabay (1080p Landscape)
 * FokusKonten Enterprise Video Engine
 * API: Pixabay Video REST API (5.000 req/jam gratis)
 * ══════════════════════════════════════════════════════════════════════════════
 */

const fs = require('fs');
const path = require('path');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

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

function getPixabayKey() {
  const vault = getStudioApiVault();
  if (vault && Array.isArray(vault.pixabay_keys)) {
    for (const item of vault.pixabay_keys) {
      if (item && item.is_active !== false && item.key) {
        return item.key.trim();
      }
    }
  }
  return process.env.PIXABAY_API_KEY ? process.env.PIXABAY_API_KEY.trim() : null;
}

async function searchPixabayVideo(query, orientation = 'horizontal') {
  const apiKey = getPixabayKey();
  if (!apiKey) {
    throw new Error('[PIXABAY] Kunci API Pixabay belum dikonfigurasi di Smart Vidkon Automation. Tambahkan di menu API Key Vault.');
  }
  const orientParam = orientation === 'vertical' ? 'vertical' : 'horizontal';
  const url = `https://pixabay.com/api/videos/?key=${apiKey}&q=${encodeURIComponent(query)}&orientation=${orientParam}&video_type=film&per_page=10`;
  const res = await fetch(url);
  if (!res.ok) {
    throw new Error(`Gagal memanggil API Pixabay (HTTP ${res.status}): ${await res.text()}`);
  }
  const data = await res.json();
  if (!data.hits || data.hits.length === 0) {
    return null;
  }

  const isVert = orientParam === 'vertical';

  // Pilih video pertama dengan orientasi dan ukuran efisien (< 35 MB)
  for (const hit of data.hits) {
    const vList = hit.videos;
    const matchLarge = isVert ? (vList.large && vList.large.height >= vList.large.width) : (vList.large && vList.large.width >= vList.large.height);
    if (matchLarge && vList.large.size < 35 * 1024 * 1024) {
      return { id: hit.id, url: vList.large.url, tags: hit.tags, width: vList.large.width, height: vList.large.height };
    }
    const matchMed = isVert ? (vList.medium && vList.medium.height >= vList.medium.width) : (vList.medium && vList.medium.width >= vList.medium.height);
    if (matchMed) {
      return { id: hit.id, url: vList.medium.url, tags: hit.tags, width: vList.medium.width, height: vList.medium.height };
    }
  }

  // Fallback ke hit pertama
  const first = data.hits[0];
  const urlFallback = (first.videos.large && first.videos.large.url) || first.videos.medium.url;
  return { id: first.id, url: urlFallback, tags: first.tags };
}

async function downloadFile(url, destPath) {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status} saat mengunduh video`);
  const buf = Buffer.from(await res.arrayBuffer());
  fs.writeFileSync(destPath, buf);
  return buf.length;
}

async function batchDownloadSceneFootage({ scenesKeywords, outputDir, orientation = 'horizontal' }) {
  fs.mkdirSync(outputDir, { recursive: true });
  console.log(`[Pixabay Bot] Memulai pengunduhan ${scenesKeywords.length} adegan video (${orientation}) ke: ${outputDir}`);

  const results = [];
  for (let i = 0; i < scenesKeywords.length; i++) {
    const sceneNum = String(i + 1).padStart(2, '0');
    const keyword = scenesKeywords[i];
    const targetFile = path.join(outputDir, `${sceneNum}_scene.mp4`);

    process.stdout.write(`[Pixabay Bot] [${i + 1}/${scenesKeywords.length}] Mencari: "${keyword}"... `);

    try {
      let videoInfo = await searchPixabayVideo(keyword, orientation);
      if (!videoInfo) {
        // Fallback pencarian lebih umum jika spesifik tidak ada
        const fallbackWord = keyword.split(' ')[0] || 'technology';
        process.stdout.write(`(fallback: "${fallbackWord}")... `);
        videoInfo = await searchPixabayVideo(fallbackWord, orientation);
      }

      if (!videoInfo || !videoInfo.url) {
        console.log(`GAGAL (tidak ditemukan video)`);
        results.push({ scene: i + 1, status: 'NOT_FOUND' });
        continue;
      }

      const sizeBytes = await downloadFile(videoInfo.url, targetFile);
      const sizeMB = (sizeBytes / (1024 * 1024)).toFixed(2);
      console.log(`SUKSES (${sizeMB} MB) [ID: ${videoInfo.id}]`);
      results.push({ scene: i + 1, status: 'OK', file: targetFile, sizeMB });
    } catch (err) {
      console.log(`ERROR: ${err.message}`);
      results.push({ scene: i + 1, status: 'ERROR', error: err.message });
    }
  }

  console.log(`[Pixabay Bot] Selesai! Berhasil mengunduh ${results.filter(r => r.status === 'OK').length}/${scenesKeywords.length} adegan.`);
  return results;
}

// CLI usage: node fetch_stock_videos_pixabay.js <keywords_json_or_txt> <output_dir> [orientation: horizontal|vertical]
if (require.main === module) {
  const args = process.argv.slice(2);
  const inputArg = args[0];
  const outputDir = args[1];
  const orientationArg = args[2] || 'horizontal';

  if (!inputArg || !outputDir) {
    console.log('Penggunaan: node fetch_stock_videos_pixabay.js <keywords.json|keywords.txt> <output_dir> [horizontal|vertical]');
    process.exit(1);
  }

  let keywords = [];
  if (fs.existsSync(inputArg)) {
    const raw = fs.readFileSync(inputArg, 'utf8').trim();
    if (inputArg.endsWith('.json')) {
      keywords = JSON.parse(raw);
    } else {
      keywords = raw.split('\n').map(l => l.trim()).filter(Boolean);
    }
  } else {
    // Dipisah koma
    keywords = inputArg.split(',').map(s => s.trim()).filter(Boolean);
  }

  batchDownloadSceneFootage({ scenesKeywords: keywords, outputDir, orientation: orientationArg })
    .then(() => process.exit(0))
    .catch(err => {
      console.error(err);
      process.exit(1);
    });
}

module.exports = { searchPixabayVideo, batchDownloadSceneFootage };
