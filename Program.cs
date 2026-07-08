using ChineseCheckersMinimax;

const int SearchDepth = 4;  // Minimax'in kaç hamle ileri baktığı
const int MaxTurns = 200;   // Sonsuz döngü önleme: bu kadar hamlede bitmezse berabere

Console.WriteLine("=== Cin Damasi ===");
Console.WriteLine("1) Insana karsi oyna (sen vs Minimax)");
Console.WriteLine("2) Minimax vs Genetik Algoritma (otomatik mac)");
Console.Write("Mod (1/2): ");
var mode = Console.ReadLine()?.Trim();
Console.WriteLine();

if (mode == "2")
    RunAutoMatch();
else
    RunHumanGame();

// ─────────────────────────────────────────────
// MOD 1: İnsan vs Minimax (orijinal oyun modu)
// ─────────────────────────────────────────────
void RunHumanGame()
{
    Console.WriteLine("=== Cin Damasi - Minimax ===");
    Console.WriteLine("H = Sen (Insan), C = Bilgisayar, . = bos kare");
    Console.WriteLine("Insan hedefi: sag-alt kose. Bilgisayar hedefi: sol-ust kose.");
    Console.WriteLine("Hamle girisi: 'satir sutun satir sutun'  (ornek: 0 0 1 1)");
    Console.WriteLine();

    var board = new Board();
    var turn = Player.Human;

    while (true)
    {
        board.Print();

        if (board.HasWon(Player.Human))   { Console.WriteLine("Kazandin! Tebrikler."); break; }
        if (board.HasWon(Player.Computer)){ Console.WriteLine("Bilgisayar kazandi."); break; }

        if (turn == Player.Human)
        {
            var moves = board.GenerateMoves(Player.Human);
            if (moves.Count == 0)
            {
                Console.WriteLine("Yapabilecegin hamle yok, siran gecti.");
                turn = Player.Computer;
                continue;
            }

            Console.Write("Hamlen (satir sutun satir sutun): ");
            var line = Console.ReadLine();
            if (line == null) break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 ||
                !int.TryParse(parts[0], out int fr) || !int.TryParse(parts[1], out int fc) ||
                !int.TryParse(parts[2], out int tr) || !int.TryParse(parts[3], out int tc))
            {
                Console.WriteLine("Gecersiz giris. Ornek: 0 0 1 1");
                continue;
            }

            var chosen = moves.FirstOrDefault(m =>
                m.FromR == fr && m.FromC == fc && m.ToR == tr && m.ToC == tc);

            if (chosen == null)
            {
                Console.WriteLine("Bu gecersiz bir hamle. Gecerli hamleler:");
                foreach (var m in moves) Console.WriteLine("  " + m);
                continue;
            }

            board = board.Apply(chosen, Player.Human);
            turn = Player.Computer;
        }
        else
        {
            Console.WriteLine("Bilgisayar dusunuyor...");
            var (move, score) = Ai.ChooseBestMove(board, SearchDepth);
            Console.WriteLine($"Bilgisayar oynadi: {move}  (skor: {score})");
            board = board.Apply(move, Player.Computer);
            turn = Player.Human;
        }

        Console.WriteLine();
    }
}

// ─────────────────────────────────────────────────────────
// MOD 2: Minimax (C) vs Genetik Algoritma (H) — otomatik
// ─────────────────────────────────────────────────────────
void RunAutoMatch()
{
    Console.WriteLine("=== Minimax (C) vs Genetik Algoritma (H) ===");
    Console.WriteLine($"Minimax derinligi: {SearchDepth}  |  Tur limiti: {MaxTurns}");
    Console.WriteLine();

    var board = new Board();
    var turn = Player.Human; // GA ilk oynuyor (İnsan rolü)
    int turnCount = 0;

    while (turnCount < MaxTurns)
    {
        board.Print();

        // Kazanma kontrolü: önceki hamle birini kazandırdıysa bitir
        if (board.HasWon(Player.Human))
        {
            Console.WriteLine($"\nKAZANAN: Genetik Algoritma (H) — {turnCount} hamle");
            return;
        }
        if (board.HasWon(Player.Computer))
        {
            Console.WriteLine($"\nKAZANAN: Minimax (C) — {turnCount} hamle");
            return;
        }

        if (turn == Player.Human)
        {
            // Genetik Algoritma Human rolünde oynuyor
            var moves = board.GenerateMoves(Player.Human);
            if (moves.Count == 0)
            {
                Console.WriteLine("GA'nin hamlesi yok, tur gecti.");
                turn = Player.Computer;
                continue;
            }
            Console.Write($"[{turnCount + 1}] GA dusunuyor... ");
            var move = GeneticAi.ChooseBestMove(board, Player.Human);
            Console.WriteLine($"GA oynadi: {move}");
            board = board.Apply(move, Player.Human);
            turn = Player.Computer;
        }
        else
        {
            // Minimax Computer rolünde oynuyor
            var moves = board.GenerateMoves(Player.Computer);
            if (moves.Count == 0)
            {
                Console.WriteLine("Minimax'in hamlesi yok, tur gecti.");
                turn = Player.Human;
                continue;
            }
            Console.Write($"[{turnCount + 1}] Minimax dusunuyor... ");
            var (move, score) = Ai.ChooseBestMove(board, SearchDepth);
            Console.WriteLine($"Minimax oynadi: {move}  (skor: {score})");
            board = board.Apply(move, Player.Computer);
            turn = Player.Human;
        }

        turnCount++;
        Console.WriteLine();
    }

    // Tur limitine ulaşıldı
    board.Print();
    Console.WriteLine($"\nTur limiti ({MaxTurns}) doldu. Kazanan yok (berabere).");
}
