namespace ChineseCheckersMinimax;

public static class Ai
{
    // Verilen oyuncu perspektifinden tahtayı puanlar.
    // perspective = Player.Computer (varsayılan): yüksek puan Bilgisayar için iyi.
    // perspective = Player.Human: yüksek puan İnsan için iyi (skoru çevirir).
    // Varsayılan parametre sayesinde mevcut Minimax kodu değişmeden çalışır.
    public static int Evaluate(Board b, Player perspective = Player.Computer)
    {
        int score = 0;

        // Bilgisayar hedefi sol-üst köşe, İnsan hedefi sağ-alt köşe.
        // Her taşın hedefe olan Manhattan uzaklığı düşükse puan yükselir.
        int maxDist = (Board.Size - 1) * 2;

        for (int r = 0; r < Board.Size; r++)
        {
            for (int c = 0; c < Board.Size; c++)
            {
                var p = b.At(r, c);
                if (p == Player.Computer)
                {
                    int dist = ClosestDist(r, c, Board.ComputerTarget);
                    score += (maxDist - dist);                       // hedefe yaklaş = artı
                    if (IsIn(r, c, Board.ComputerTarget)) score += 10; // hedefe girme bonusu
                }
                else if (p == Player.Human)
                {
                    int dist = ClosestDist(r, c, Board.HumanTarget);
                    score -= (maxDist - dist);                       // rakip yaklaşıyor = eksi
                    if (IsIn(r, c, Board.HumanTarget)) score -= 10;
                }
            }
        }

        if (b.HasWon(Player.Computer)) score += 1000;
        if (b.HasWon(Player.Human)) score -= 1000;

        // İnsan perspektifinden sorulursa skoru çevir: yüksek = İnsan için iyi.
        return perspective == Player.Computer ? score : -score;
    }

    private static int ClosestDist(int r, int c, (int r, int c)[] targets)
    {
        int best = int.MaxValue;
        foreach (var (tr, tc) in targets)
        {
            int d = Math.Abs(r - tr) + Math.Abs(c - tc); // manhattan uzaklığı
            if (d < best) best = d;
        }
        return best;
    }

    private static bool IsIn(int r, int c, (int r, int c)[] cells)
    {
        foreach (var (cr, cc) in cells)
            if (cr == r && cc == c) return true;
        return false;
    }

    // Minimax + alfa-beta budama. Her zaman Bilgisayar perspektifinden değerlendirir
    // (maximizing=true → Bilgisayar oynuyor; false → İnsan oynuyor).
    public static int Minimax(Board b, int depth, bool maximizing, int alpha, int beta)
    {
        if (depth == 0 || b.HasWon(Player.Computer) || b.HasWon(Player.Human))
            return Evaluate(b); // perspektif belirtilmez → varsayılan = Computer

        Player p = maximizing ? Player.Computer : Player.Human;
        var moves = b.GenerateMoves(p);
        if (moves.Count == 0) return Evaluate(b);

        if (maximizing)
        {
            int best = int.MinValue;
            foreach (var m in moves)
            {
                var child = b.Apply(m, p);
                int val = Minimax(child, depth - 1, false, alpha, beta);
                best = Math.Max(best, val);
                alpha = Math.Max(alpha, best);
                if (beta <= alpha) break; // budama
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            foreach (var m in moves)
            {
                var child = b.Apply(m, p);
                int val = Minimax(child, depth - 1, true, alpha, beta);
                best = Math.Min(best, val);
                beta = Math.Min(beta, best);
                if (beta <= alpha) break; // budama
            }
            return best;
        }
    }

    // Bilgisayar için en iyi hamleyi seçer. Seçim gerekçesini de döndürür.
    public static (Move move, int score) ChooseBestMove(Board b, int depth)
    {
        var moves = b.GenerateMoves(Player.Computer);
        Move? best = null;
        int bestScore = int.MinValue;

        foreach (var m in moves)
        {
            var child = b.Apply(m, Player.Computer);
            int score = Minimax(child, depth - 1, false, int.MinValue, int.MaxValue);
            if (score > bestScore)
            {
                bestScore = score;
                best = m;
            }
        }

        return (best!, bestScore);
    }
}
