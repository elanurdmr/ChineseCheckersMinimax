using System.Text;

namespace ChineseCheckersMinimax;

// İki oyuncu: İnsan ve Bilgisayar
public enum Player { None = 0, Human = 1, Computer = 2 }

// Bir hamle: başlangıç karesinden, izlenen kareler dizisiyle bitiş karesine.
// Path[0] = başlangıç, Path[Son] = varış. Zincirleme zıplamada aradaki
// duraklar da Path içinde tutulur (gösterim için).
public record Move(int FromR, int FromC, int ToR, int ToC, List<(int r, int c)> Path)
{
    public override string ToString() => $"({FromR},{FromC}) -> ({ToR},{ToC})";
}

public class Board
{
    public const int Size = 5;
    public const int PiecesPerPlayer = 3;

    // grid[r,c] hangi oyuncunun taşını tutuyor (None = boş)
    private readonly Player[,] grid = new Player[Size, Size];

    // İnsan başlangıç ve hedef köşeleri (sol-üst -> sağ-alt)
    public static readonly (int r, int c)[] HumanStart =
        { (0, 0), (0, 1), (1, 0) };
    public static readonly (int r, int c)[] HumanTarget =
        { (4, 4), (4, 3), (3, 4) };

    // Bilgisayar başlangıç ve hedef köşeleri (sağ-alt -> sol-üst)
    public static readonly (int r, int c)[] ComputerStart =
        { (4, 4), (4, 3), (3, 4) };
    public static readonly (int r, int c)[] ComputerTarget =
        { (0, 0), (0, 1), (1, 0) };

    // 8 yön: yatay, dikey, çapraz
    private static readonly (int dr, int dc)[] Dirs =
    {
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    };

    public Board()
    {
        foreach (var (r, c) in HumanStart) grid[r, c] = Player.Human;
        foreach (var (r, c) in ComputerStart) grid[r, c] = Player.Computer;
    }

    // Derin kopya (Minimax'in ağaçta ilerlerken tahtayı bozmaması için)
    public Board Clone()
    {
        var b = new Board();
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                b.grid[r, c] = grid[r, c];
        return b;
    }

    public Player At(int r, int c) => grid[r, c];

    private static bool InBounds(int r, int c) =>
        r >= 0 && r < Size && c >= 0 && c < Size;

    // Verilen oyuncunun yapabileceği TÜM geçerli hamleleri üretir.
    public List<Move> GenerateMoves(Player p)
    {
        var moves = new List<Move>();

        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                if (grid[r, c] != p) continue;

                // 1) Basit kayma: boş komşuya
                foreach (var (dr, dc) in Dirs)
                {
                    int nr = r + dr, nc = c + dc;
                    if (InBounds(nr, nc) && grid[nr, nc] == Player.None)
                    {
                        moves.Add(new Move(r, c, nr, nc,
                            new List<(int, int)> { (r, c), (nr, nc) }));
                    }
                }

                // 2) Zıplama (zincirleme dahil): geriye dönük arama
                var visited = new HashSet<(int, int)> { (r, c) };
                CollectJumps(r, c, r, c, new List<(int, int)> { (r, c) }, visited, moves);
            }
        }

        return moves;
    }

    // Bir taşın (origR,origC) başlayıp, mevcut konumdan (curR,curC)
    // yapabileceği tüm zincirleme zıplamaları toplar.
    private void CollectJumps(int origR, int origC, int curR, int curC,
        List<(int, int)> path, HashSet<(int, int)> visited, List<Move> moves)
    {
        foreach (var (dr, dc) in Dirs)
        {
            int overR = curR + dr, overC = curC + dc;      // üzerinden atlanan kare
            int landR = curR + 2 * dr, landC = curC + 2 * dc; // inilen kare

            if (!InBounds(landR, landC)) continue;
            if (grid[overR, overC] == Player.None) continue;   // atlanacak taş yok
            if (grid[landR, landC] != Player.None) continue;   // iniş dolu
            if (visited.Contains((landR, landC))) continue;    // döngü engeli

            // Geçici olarak taşı ilerlet, döngüyü sürdür
            visited.Add((landR, landC));
            var newPath = new List<(int, int)>(path) { (landR, landC) };

            moves.Add(new Move(origR, origC, landR, landC, newPath));

            CollectJumps(origR, origC, landR, landC, newPath, visited, moves);

            visited.Remove((landR, landC));
        }
    }

    // Bir hamleyi tahtaya uygular (yeni bir kopya döndürür).
    public Board Apply(Move m, Player p)
    {
        var b = Clone();
        b.grid[m.FromR, m.FromC] = Player.None;
        b.grid[m.ToR, m.ToC] = p;
        return b;
    }

    // Kazanma: oyuncunun tüm taşları kendi hedef köşesinde mi?
    public bool HasWon(Player p)
    {
        var target = p == Player.Human ? HumanTarget : ComputerTarget;
        foreach (var (r, c) in target)
            if (grid[r, c] != p) return false;
        return true;
    }

    // Konsola tahtayı çizer. H = İnsan, C = Bilgisayar, . = boş
    public void Print()
    {
        var sb = new StringBuilder();
        sb.Append("    ");
        for (int c = 0; c < Size; c++) sb.Append($"{c} ");
        sb.AppendLine();
        for (int r = 0; r < Size; r++)
        {
            sb.Append($" {r}  ");
            for (int c = 0; c < Size; c++)
            {
                char ch = grid[r, c] switch
                {
                    Player.Human => 'H',
                    Player.Computer => 'C',
                    _ => '.'
                };
                sb.Append($"{ch} ");
            }
            sb.AppendLine();
        }
        Console.WriteLine(sb.ToString());
    }
}
