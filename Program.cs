using ChineseCheckersMinimax;

const int SearchDepth = 4; // Minimaxın kaç hamle ileri baktığı

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

    if (board.HasWon(Player.Human)) { Console.WriteLine("Kazandin! Tebrikler."); break; }
    if (board.HasWon(Player.Computer)) { Console.WriteLine("Bilgisayar kazandi."); break; }

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
