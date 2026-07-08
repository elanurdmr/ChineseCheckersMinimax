# Çin Daması - Minimax (.NET Console)

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

## Alfa-Beta Budama nedir?

Minimaxın gereksiz yere her dalı sonuna kadar aramasını engeller. alpha bilgisayarın, beta insanın o ana kadar bulduğu en iyi skordur. beta <= alpha olduğunda rakip zaten daha iyi bir seçeneğe sahip demektir, o yüzden kalan hamleler atlanır. Sonucu değiştirmez, sadece daha hızlı bulunmasını sağlar.

## Skor mantığı

- Bilgisayar taşı hedefe yaklaştıkça **+puan** (Manhattan uzaklığı ile), hedefe
  girerse **+10** bonus.
- İnsan taşı kendi hedefine yaklaştıkça bilgisayar için **-puan**, girerse **-10**.
- Kazanan taraf için **±1000**.

`Ai.ChooseBestMove`, tüm bilgisayar hamlelerini dener, her birini Minimax ile puanlar
ve **en yüksek skorlu** hamleyi seçer.

## Örnek oyun durumları

### Örnek 1 - Açılış hamlesi

```
    0 1 2 3 4
 0  H H . . .
 1  H . . . .
 2  . . . . .
 3  . . . . C
 4  . . . C C
```

```
(3,4)->(2,3)  skor -1
```

**Neden?** Bu hamle bilgisayarın hedefine (sol-üst köşe) olan uzaklığını `6`'dan
`4`'e düşürür. Diğer hamleler ya uzaklığı değiştirmiyor ya da artırıyor, bu yüzden
hedefe yaklaşan bu hamle daha yüksek skor alıyor.

### Örnek 2 - Zıplama fırsatı

```
    0 1 2 3 4
 0  . . . . .
 1  . . . . .
 2  . . . H .
 3  . . . . C
 4  . . . C C
```

Bilgisayarın `(3,4)` taşı önünde bir insan taşı `(2,3)` varken:

```
(3,4)->(1,2)  skor 3
```

**Neden?** Kayma tek kare ilerletirken, insan taşının üzerinden zıplama iki kare
birden hedefe yaklaştırır. Uzaklık daha çok azaldığı için skor daha yüksektir ve
Minimax bu hamleyi seçer.

### Örnek 3 - Kazanan hamle

```
    0 1 2 3 4
 0  . C . . .
 1  C C . . .
 2  . . . . .
 3  . . . . .
 4  . . . . .
```

Bilgisayarın son taşı hedef köşenin bir adım yanındaysa ve oraya gidebiliyorsa:

```
(1,1)->(0,0)  skor 1000
```

**Neden?** Bu hamle bilgisayarın üç taşını da hedef köşeye `(0,0),(0,1),(1,0)`
tamamlar ve oyunu kazandırır. `Evaluate` fonksiyonu kazanma durumuna `+1000` verdiği
için bu hamle her zaman diğer tüm hamlelerden daha yüksek skor alır ve kesin olarak
seçilir.

## Genetik Algoritma

`dotnet run` başladığında **Mod 2** seçilerek Minimax'e karşı otomatik maç oynatılabilir.
Genetik Algoritma (GA) **İnsan (H)** rolünü, Minimax **Bilgisayar (C)** rolünü üstlenir.

### Nasıl Çalışır?

- **Kromozom:** Oyuncunun 3 hamlelik ardışık planı.
- **Fitness:** Plan uygulandıktan sonraki tahtanın `Evaluate(board, Player.Human)` skoru.
- **Başlatma:** 20 rastgele geçerli plan oluşturulur.
- **Seçim:** Turnuva seçimi (3 birey yarışır, en iyisi kazanır).
- **Çaprazlama:** Tek noktalı; bir ebeveynin başı + diğerinin sonu birleştirilir.
- **Mutasyon (olasılık 0.35):** Rastgele bir noktadan sonrası yeniden rastgele üretilir.
  Bu, çaprazlamayla bozulmuş geçersiz hamleleri de onarır.
- **Elitizm:** En iyi birey bir sonraki nesle doğrudan geçer.
- 15 nesil evrimleşir; son nesildeki en iyi planın **sadece ilk hamlesi** gerçek oyunda oynanır.

### Minimax'ten Farkı

| | Minimax | Genetik Algoritma |
|---|---|---|
| Rakibin hamlelerini hesaba katar mı? | **Evet** | **Hayır** |
| Arama yöntemi | Oyun ağacı (tam, alfa-beta budamalı) | Evrimsel (sezgisel) |
| Optimal hamleyi garantiler mi? | Derinlik dahilinde **evet** | **Hayır** |

GA'nın temel zayıflığı: yalnızca **kendi hamlelerini simüle eder**, rakibin ne oynayacağını
düşünmez. Bu yüzden rakibinin en iyi cevabını planlayan Minimax'e karşı dezavantajlıdır.

### Gerçek Maç Sonucu

**KAZANAN: Minimax (C) — 54 hamlede**

Minimax, taşlarını köşeye sistematik biçimde taşırken GA kısmen ilerledi ama
rakibin pozisyonunu hesaba katmadığı için birden fazla kez geri adım attı.
Minimax'in son hamlesiyle üçüncü taşı da `(0,0)-(0,1)-(1,0)` hedef köşesine girdi.

## Dosya yapısı

- `Program.cs` – mod seçimi, oyun döngüsü (insan girişi / otomatik maç).
- `Game.cs` – `Board`, `Move`, `Player`; hamle üretimi, zıplama/zincir, kazanma, çizim.
- `Ai.cs` – `Evaluate` (perspektif parametreli) ve alfa-beta budamalı `Minimax`.
- `GeneticAi.cs` – Genetik Algoritma tabanlı hamle seçici.
