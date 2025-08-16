// 🧪 ФИНАЛЬНО ПРАВИЛЬНЫЙ тест RTP для краш-игры (исправлено)
// Тестируем математику на 1,000,000 попыток с целевым RTP 96% (house edge 4%)

console.log("🎮 ФИНАЛЬНО ПРАВИЛЬНЫЙ тест RTP для краш-игры (исправлено)");
console.log("=".repeat(50));

// ========= ПАРАМЕТРЫ СИМУЛЯЦИИ =========
const ROUNDS = 1_000_000;
const BET_AMOUNT = 100;
const MIN_MULT = 1.01;
const MAX_MULT = 1000;
const EPSILON = 0.04; // дом. преимущество (4%), => целевой RTP = 1 - EPSILON = 0.96

// ========= МАТЕМАТИКА КРАШ-ИГРЫ =========
// Распределение с функцией выживания P(M >= m) = (1 - EPSILON)/m, m >= 1
// Выборка: M = (1 - EPSILON) / (1 - U), U ~ Uniform(0,1)
function generateCrashPoint() {
  const u = Math.random();
  let m = (1 - EPSILON) / (1 - u);
  if (m < MIN_MULT) m = MIN_MULT;
  if (m > MAX_MULT) m = MAX_MULT;
  return m;
}

// ========= ПОВЕДЕНИЕ ИГРОКОВ =========
// Игрок заранее выбирает цель кешаута target; выигрыш, если crash >= target
function sampleTarget_standard() {
  const r = Math.random();
  if (r < 0.55) return 1.10 + Math.random() * 1.40; // 1.10–2.50
  if (r < 0.85) return 2.50 + Math.random() * 2.50; // 2.50–5.00
  if (r < 0.97) return 5.00 + Math.random() * 5.00; // 5.00–10.00
  return 10.00 + Math.random() * 40.00;             // 10.00–50.00
}

function simulatePlayerBehavior(crashPoint) {
  const target = sampleTarget_standard();
  return crashPoint >= target ? target : 0; // вернуть множитель кешаута (или 0 при проигрыше)
}

// ========= УТИЛИТЫ =========
function formatNumber(n) {
  return n.toLocaleString("ru-RU", { maximumFractionDigits: 3 });
}
function formatMs(ms) {
  return (ms / 1000).toFixed(2) + " секунд";
}

// ========= СТАТИСТИКА =========
let totalBets = 0;
let totalPayouts = 0;
let wins = 0;
let losses = 0;
let maxCrash = 0;
let maxWin = 0;

const bins = [
  { name: "1.01-2.00", from: 1.01, to: 2.00, count: 0 },
  { name: "2.01-5.00", from: 2.01, to: 5.00, count: 0 },
  { name: "5.01-10.00", from: 5.01, to: 10.00, count: 0 },
  { name: "10.01-20.00", from: 10.01, to: 20.00, count: 0 },
  { name: "20.01-50.00", from: 20.01, to: 50.00, count: 0 },
  { name: "50.01-100.00", from: 50.01, to: 100.00, count: 0 },
  { name: "100.01+", from: 100.01, to: Infinity, count: 0 },
];

const t0 = Date.now();

for (let i = 1; i <= ROUNDS; i++) {
  const crash = generateCrashPoint();
  totalBets += BET_AMOUNT;

  // бины
  for (const b of bins) {
    if (crash >= b.from && crash <= b.to) { b.count++; break; }
  }

  const cashoutMult = simulatePlayerBehavior(crash);
  if (cashoutMult > 0) {
    const payout = BET_AMOUNT * cashoutMult;
    totalPayouts += payout;
    wins++;
    if (payout > maxWin) maxWin = payout;
  } else {
    losses++;
  }

  if (crash > maxCrash) maxCrash = crash;

  // лог прогресса каждые 10%
  if (i % (ROUNDS / 10) === 0) {
    const progress = (i / ROUNDS * 100).toFixed(1);
    const rtp = (totalPayouts / totalBets) * 100;
    console.log(`📈 Прогресс: ${progress}% | Текущий RTP: ${rtp.toFixed(2)}%`);
  }
}

const t1 = Date.now();
const elapsedMs = t1 - t0;

// ========= ИТОГИ =========
const RTP = totalPayouts / totalBets * 100;
const houseEdge = 100 - RTP;
const winrate = wins / ROUNDS * 100;
const speed = ROUNDS / (elapsedMs / 1000);

console.log("\\n==================================================");
console.log("📊 РЕЗУЛЬТАТЫ ФИНАЛЬНО ПРАВИЛЬНОГО ТЕСТА RTP (исправлено)");
console.log("==================================================");
console.log(`🎯 Общее количество раундов: ${formatNumber(ROUNDS)}`);
console.log(`⏱️  Время выполнения: ${formatMs(elapsedMs)}`);
console.log(`📈 Скорость: ${formatNumber(speed)} раундов/сек\\n`);

console.log("💰 ФИНАНСОВЫЕ ПОКАЗАТЕЛИ:");
console.log(`📥 Общие ставки: ${formatNumber(totalBets)}`);
console.log(`📤 Общие выплаты: ${formatNumber(totalPayouts)}`);
console.log(`🎯 RTP: ${RTP.toFixed(4)}%`);
console.log(`🏠 House Edge: ${houseEdge.toFixed(4)}%`);
console.log(`📊 Винрейт: ${winrate.toFixed(2)}%\\n`);

console.log("🏆 МАКСИМАЛЬНЫЕ ЗНАЧЕНИЯ:");
console.log(`🔥 Максимальный краш: x${maxCrash.toFixed(2)}`);
console.log(`💰 Максимальный выигрыш: ${formatNumber(maxWin.toFixed(2))}\\n`);

console.log("📊 РАСПРЕДЕЛЕНИЕ КРАШЕЙ:");
const totalCrashes = bins.reduce((s, b) => s + b.count, 0);
bins.forEach((b, idx) => {
  const pct = (b.count / totalCrashes * 100).toFixed(2);
  console.log(`${idx + 1}. ${b.name}: ${formatNumber(b.count)} (${pct}%)`);
});

console.log("\\n🎮 СТАТИСТИКА ИГР:");
console.log(`✅ Выигрыши: ${formatNumber(wins)} (${winrate.toFixed(2)}%)`);
console.log(`❌ Проигрыши: ${formatNumber(losses)} (${(100 - winrate).toFixed(2)}%)\\n`);

console.log("📈 ОЦЕНКА КАЧЕСТВА:");
const targetRTP = 96.00;
const deviation = RTP - targetRTP;
const ok = Math.abs(deviation) <= 0.5;
console.log(ok ? "✅ OK: RTP близок к целевому" : "❌ ТРЕБУЕТ ДОРАБОТКИ! RTP отличается от целевого");
console.log(`🎯 Целевой RTP: ${targetRTP.toFixed(2)}%`);
console.log(`📊 Фактический RTP: ${RTP.toFixed(4)}%`);
console.log(`📏 Отклонение: ${deviation.toFixed(4)}%`);
