# Çin Daması - Minimax Algoritması (.NET Console)

Konsolda oynanan Çin Daması Oyunu. İnsan, bilgisayara karşı oynar; bilgisayar hamlelerini
**Minimax** algoritmasıyla seçer (rastgele değil).

## Çalıştırma

```bash
dotnet run
```

.NET 8 SDK gerekir.

## Kurallar

- Tahta **5x5**, her oyuncunun **3 taşı** var.
- **İnsan (H):** sol-üstten başlar, hedefi sağ-alt köşe.
- **Bilgisayar (C):** sağ-alttan başlar, hedefi sol-üst köşe.
- Taşlar 8 yöne **kayabilir** (boş komşuya) veya **zıplayabilir** (bir taşın üzerinden
  boş kareye). Zıplamalar art arda **zincirlenebilir**.
- Geçersiz hamleler (tahta dışı, dolu kareye gitme, kural dışı atlama) engellenir;
  hatalı girişte geçerli hamle listesi gösterilir.
- Tüm taşlarını hedef köşeye taşıyan kazanır.

**Hamle girişi:** `satır sütun satır sütun` → örnek: `4 4 3 3`

## Minimax nedir?

Bilgisayar olası hamlelerini hesaplar, her birine karşı rakibin en iyi cevabını
düşünür, bunu birkaç hamle ileriye (**derinlik 4**) kadar tekrarlar. Bilgisayar
puanı **büyütmeye**, insan **küçültmeye** çalışıyormuş gibi varsayılır. Ağacın dibi
skor fonksiyonuyla puanlanır, puanlar yukarı taşınır, kökte en iyi hamle seçilir.
Hız için **alfa-beta budama** kullanılır.

## Skor mantığı

- Bilgisayar taşı hedefe yaklaştıkça **+puan** (Manhattan uzaklığı ile), hedefe
  girerse **+10** bonus.
- İnsan taşı kendi hedefine yaklaştıkça bilgisayar için **-puan**, girerse **-10**.
- Kazanan taraf için **±1000**.

`Ai.ChooseBestMove`, tüm bilgisayar hamlelerini dener, her birini Minimax ile puanlar
ve **en yüksek skorlu** hamleyi seçer.

## Örnek

Bilgisayarın `(3,4)` taşı önünde insan taşı `(2,3)` varken:

```
(3,4)->(1,2)  skor 3
```

**Neden?** Kayma tek kare ilerletirken, zıplama iki kare birden hedefe yaklaştırır;
uzaklık daha çok azaldığı için skor daha yüksektir.

## Dosya yapısı

- `Program.cs` – oyun döngüsü, insan girişi, sıra yönetimi.
- `Game.cs` – `Board`, `Move`, `Player`; hamle üretimi, zıplama/zincir, kazanma, çizim.
- `Ai.cs` – `Evaluate` skor fonksiyonu ve alfa-beta budamalı `Minimax`.
