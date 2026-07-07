# Çin Daması – Minimax (.NET Console)

Basit bir Çin daması oyunu. İnsan, bilgisayara karşı oynar. Bilgisayar hamlelerini
**Minimax** algoritmasıyla seçer, rastgele oynamaz. Amaç profesyonel bir oyun değil;
oyun kurallarını modelleyip bir algoritmanın karar vermesini göstermektir.

## Çalıştırma

```bash
dotnet run
```

.NET 8 SDK gerekir.

## Oyun kuralları

Basit tutmak için tahta küçültülmüştür:

- Tahta **5x5** karedir.
- Her oyuncunun **3 taşı** vardır.
- **İnsan (H)** sol-üst köşeden başlar: `(0,0) (0,1) (1,0)`. Hedefi **sağ-alt** köşedir.
- **Bilgisayar (C)** sağ-alt köşeden başlar: `(4,4) (4,3) (3,4)`. Hedefi **sol-üst** köşedir.
- Oyuncular **sırayla** oynar.
- Taşlar 8 yöne (yatay, dikey, çapraz) hareket eder:
  - **Kayma:** boş bir komşu kareye geçiş.
  - **Zıplama:** hemen yanındaki bir taşın üzerinden, arkasındaki boş kareye atlama.
  - **Zincirleme zıplama:** iniş sonrası yeni zıplama mümkünse tek hamlede zincirlenir.
- **Geçersiz hamleler** engellenir: tahta dışına çıkış, dolu kareye gidiş, taşı olmayan
  kareyi oynama veya kurallara uymayan atlama kabul edilmez. İnsan geçersiz hamle
  girerse geçerli hamle listesi gösterilir.
- Tüm taşlarını karşı hedef köşeye yerleştiren oyuncu **kazanır**.

## Hamle girişi

```
satır sütun satır sütun
```

Örnek: `4 4 3 3` → `(4,4)` taşını `(3,3)` karesine oynar.

## Minimax nedir?

Minimax, iki oyunculu sıralı oyunlarda karar verme algoritmasıdır. Mantığı:

1. Bilgisayar yapabileceği tüm hamleleri hesaplar.
2. Her hamle sonrası rakibin (insanın) verebileceği en iyi cevabı düşünür.
3. Rakibin cevabından sonra kendi en iyi hamlesini... diye belirli bir **derinliğe**
   kadar ağacı kurar.
4. Bilgisayar kendi puanını **büyütmeye** (maximizing), rakibin ise **küçültmeye**
   (minimizing) çalıştığını varsayar.
5. Ağacın dibindeki tahtalar bir **skor fonksiyonu** ile puanlanır, bu puanlar yukarı taşınır ve kökte en iyi hamle seçilir.

Bu projede derinlik `4`'tür (`Program.cs` içinde `SearchDepth`). Performans için
**alfa-beta budama** eklenmiştir: sonucu değiştirmeyeceği kesinleşen dalların
aranması durdurulur.

## Skor mantığı (Evaluate)

Skor bilgisayar açısından hesaplanır (yüksek = bilgisayar için iyi):

- Her **bilgisayar** taşı hedefe (sol-üst) yaklaştıkça **artı** puan
  (Manhattan uzaklığı kullanılır).
- Hedef köşedeki her bilgisayar taşı için **+10** bonus.
- Her **insan** taşı kendi hedefine yaklaştıkça bilgisayar için **eksi** puan.
- Hedefe girmiş insan taşı için **-10**.
- Bilgisayar oyunu kazanırsa **+1000**, insan kazanırsa **-1000**.

## Bilgisayar hamleyi nasıl seçer?

`Ai.ChooseBestMove` tüm bilgisayar hamlelerini üretir, her birini uygular ve ortaya
çıkan tahtayı `Minimax` ile puanlar. **En yüksek** skorlu hamle seçilir.

## Örnek oyun durumları

### Örnek A — Başlangıç konumu

Başlangıç tahtası:

```
    0 1 2 3 4
 0  H H . . .
 1  H . . . .
 2  . . . . .
 3  . . . . C
 4  . . . C C
```

Bilgisayar açılışta değerlendirdiği hamlelerden bazıları (derinlik 3, gerçek çıktı):

```
(4,4)->(4,2)  skor -1
(4,4)->(3,3)  skor -1
(4,4)->(2,4)  skor -1
(4,3)->(3,2)  skor -1
(3,4)->(2,3)  skor -1
```

Açılışta taşlar birbirine simetrik uzaklıkta olduğundan skorlar yakındır; bilgisayar
hedefe (sol-üst) doğru ilerleyen, uzaklığı azaltan bir hamleyi seçer — örneğin
`(3,4)->(2,3)`, uzaklığı `6`'dan `4`'e düşürür.

**Neden?** Skor fonksiyonu hedefe yaklaşan taşa artı puan verdiği için, geriye veya
yana giden hamleler yerine sol-üste doğru ilerleyen hamle daha yüksek puan alır.

### Örnek B — Zıplama fırsatı

Yapay bir durumda bilgisayarın `(3,4)` taşının önünde bir insan taşı `(2,3)` var:

```
    0 1 2 3 4
 0  . . . . .
 1  . . . . .
 2  . . . H .
 3  . . . . C
 4  . . . C C
```

Bilgisayarın seçtiği hamle (gerçek çıktı):

```
(3,4)->(1,2)  skor 3
```

**Neden?** Kayma tek kare ilerletirken, insan taşının üzerinden **zıplama** taşı iki
kare birden hedefe (sol-üste) yaklaştırır. Uzaklık daha çok azaldığı için skor daha
yüksektir ve Minimax bu hamleyi seçer.

## Dosya yapısı

- `Program.cs` – oyun döngüsü, insan girişi, sıra yönetimi.
- `Game.cs` – `Board`, `Move`, `Player`; hamle üretimi, zıplama/zincir, kazanma, çizim.
- `Ai.cs` – `Evaluate` skor fonksiyonu ve alfa-beta budamalı `Minimax`.
