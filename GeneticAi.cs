namespace ChineseCheckersMinimax;

// Genetik Algoritma tabanlı hamle seçici.
// Minimax'ten temel farkı: rakibin hamlelerini hesaba katmaz.
// Sadece kendi bakış açısından "en iyi gelecek planını" arar.
public static class GeneticAi
{
    private static readonly Random rng = new();

    // --- Parametreler ---
    // PlanLength: kromozom = kaç hamle ileri bakılacağı
    // PopSize: popülasyon büyüklüğü
    // Generations: kaç nesil evrimleşileceği
    // TournamentK: turnuva seçiminde yarışan birey sayısı (büyük k → seçim baskısı artar)
    // MutationRate: mutasyon olasılığı
    private const int PlanLength = 3;
    private const int PopSize = 20;
    private const int Generations = 15;
    private const int TournamentK = 3;
    private const double MutationRate = 0.35;

    // Verilen oyuncu için Genetik Algoritma ile en iyi hamleyi seçer.
    // Döndürülen hamle, son nesildeki en iyi planın ilk adımıdır.
    public static Move ChooseBestMove(Board b, Player player)
    {
        // Güvenlik: geçerli hamle yoksa istisna fırlat
        var legalFirst = b.GenerateMoves(player);
        if (legalFirst.Count == 0)
            throw new InvalidOperationException($"{player} için oynanabilir hamle yok.");

        // İlk nesli rastgele oluştur
        var population = InitPopulation(b, player);

        for (int gen = 0; gen < Generations; gen++)
        {
            // Elitizm: en iyi birey bir sonraki nesle doğrudan geçer.
            // Bu, bulunan en iyi çözümün yeni nesil tarafından bozulmamasını garantiler.
            var elite = BestChromosome(population, b, player);
            var nextGen = new List<List<Move>>(PopSize) { elite };

            while (nextGen.Count < PopSize)
            {
                // Turnuva seçimiyle iki ebeveyn seç
                var p1 = TournamentSelect(population, b, player);
                var p2 = TournamentSelect(population, b, player);

                // Tek noktalı çaprazlama: p1'in başı + p2'nin sonu
                var child = Crossover(p1, p2);

                // Rastgele mutasyon: bir noktadan sonrasını yeniden üret
                if (rng.NextDouble() < MutationRate)
                    child = Mutate(child, b, player);

                if (child.Count == 0)
                    child = RandomChromosome(b, player);

                nextGen.Add(child);
            }

            population = nextGen;
        }

        // Son nesildeki en iyi planın ilk hamlesini al
        var bestPlan = BestChromosome(population, b, player);

        // İlk hamlenin mevcut tahtada geçerli olup olmadığını doğrula.
        // Çaprazlama/mutasyon sonucunda geçersiz bir ilk hamle üretilmiş olabilir.
        if (bestPlan.Count > 0 && legalFirst.Any(m =>
                m.FromR == bestPlan[0].FromR && m.FromC == bestPlan[0].FromC &&
                m.ToR == bestPlan[0].ToR   && m.ToC == bestPlan[0].ToC))
        {
            return bestPlan[0];
        }

        // Geri dönüş: rastgele geçerli hamle
        return legalFirst[rng.Next(legalFirst.Count)];
    }

    // Başlangıç popülasyonunu rastgele hamle dizileriyle doldurur
    private static List<List<Move>> InitPopulation(Board b, Player player)
    {
        var pop = new List<List<Move>>(PopSize);
        for (int i = 0; i < PopSize; i++)
            pop.Add(RandomChromosome(b, player));
        return pop;
    }

    // PlanLength uzunluğunda rastgele geçerli hamle dizisi üretir.
    // Her adımda tahtanın anlık durumuna bakarak geçerli hamlelerden rastgele seçer.
    private static List<Move> RandomChromosome(Board b, Player player)
    {
        var plan = new List<Move>(PlanLength);
        var sim = b.Clone();

        for (int i = 0; i < PlanLength; i++)
        {
            var moves = sim.GenerateMoves(player);
            if (moves.Count == 0) break;
            var m = moves[rng.Next(moves.Count)];
            plan.Add(m);
            sim = sim.Apply(m, player);
        }
        return plan;
    }

    // Fitness: planı sırayla uygulayıp son tahtanın Evaluate skorunu döndürür.
    // Geçersiz bir hamleyle karşılaşılırsa simülasyon orada durur ve o anki tahta puanlanır.
    // Rakibin hamleleri görmezden gelinir — bu GA'nın bilinen zayıflığıdır.
    private static int Fitness(List<Move> plan, Board board, Player player)
    {
        var sim = board.Clone();
        foreach (var m in plan)
        {
            var legal = sim.GenerateMoves(player);
            // Çaprazlama veya mutasyon bu hamleyi geçersiz kılmış olabilir
            var found = legal.FirstOrDefault(x =>
                x.FromR == m.FromR && x.FromC == m.FromC &&
                x.ToR   == m.ToR   && x.ToC   == m.ToC);
            if (found == null) break;
            sim = sim.Apply(found, player);
        }
        // Ai.Evaluate(sim, player): yüksek = verilen oyuncu için iyi
        return Ai.Evaluate(sim, player);
    }

    // Turnuva seçimi: TournamentK birey arasından en yüksek fitness'a sahip olanı döndürür
    private static List<Move> TournamentSelect(List<List<Move>> pop, Board b, Player player)
    {
        List<Move>? best = null;
        int bestFit = int.MinValue;

        for (int i = 0; i < TournamentK; i++)
        {
            var candidate = pop[rng.Next(pop.Count)];
            int fit = Fitness(candidate, b, player);
            if (fit > bestFit) { bestFit = fit; best = candidate; }
        }
        return best!;
    }

    // Tek noktalı çaprazlama: p1'in ilk kısmı + p2'nin geri kalanı
    private static List<Move> Crossover(List<Move> p1, List<Move> p2)
    {
        if (p1.Count == 0) return new List<Move>(p2);
        if (p2.Count == 0) return new List<Move>(p1);

        // Çaprazlama noktası her iki ebeveynin geçerli sınırları içinde seçilir
        int point = rng.Next(Math.Min(p1.Count, p2.Count) + 1);
        var child = new List<Move>(p1.Take(point));
        child.AddRange(p2.Skip(point));
        return child;
    }

    // Mutasyon: rastgele bir noktadan sonrasını yeniden üretir.
    // Bu, çaprazlamayla oluşmuş geçersiz hamle dizilerini de onarır.
    private static List<Move> Mutate(List<Move> plan, Board board, Player player)
    {
        if (plan.Count == 0) return RandomChromosome(board, player);

        // Mutasyon noktasına kadar korunan kısmı simüle ederek geçerliliğini doğrula
        int point = rng.Next(plan.Count);
        var sim = board.Clone();
        var prefix = new List<Move>();

        foreach (var m in plan.Take(point))
        {
            var legal = sim.GenerateMoves(player);
            var found = legal.FirstOrDefault(x =>
                x.FromR == m.FromR && x.FromC == m.FromC &&
                x.ToR   == m.ToR   && x.ToC   == m.ToC);
            if (found == null) break; // geçersiz hamle, simülasyonu burada kes
            prefix.Add(found);
            sim = sim.Apply(found, player);
        }

        // Kalan kısmı PlanLength'e ulaşana dek rastgele üret
        for (int i = prefix.Count; i < PlanLength; i++)
        {
            var moves = sim.GenerateMoves(player);
            if (moves.Count == 0) break;
            var m = moves[rng.Next(moves.Count)];
            prefix.Add(m);
            sim = sim.Apply(m, player);
        }
        return prefix;
    }

    // Popülasyondaki en yüksek fitness'a sahip bireyi döndürür
    private static List<Move> BestChromosome(List<List<Move>> pop, Board b, Player player)
        => pop.OrderByDescending(c => Fitness(c, b, player)).First();
}
