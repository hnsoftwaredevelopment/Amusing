namespace Amusing.Facturatie;

public sealed record Deelnamebedrag
{
    public Deelnamebedrag(decimal deelnemersInclBtw, decimal podiumInclBtw, decimal btwPercentage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deelnemersInclBtw);
        ArgumentOutOfRangeException.ThrowIfNegative(podiumInclBtw);
        ArgumentOutOfRangeException.ThrowIfNegative(btwPercentage);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(btwPercentage, 100m);

        DeelnemersInclBtw = deelnemersInclBtw;
        PodiumInclBtw = podiumInclBtw;
        BtwPercentage = btwPercentage;
    }

    public decimal DeelnemersInclBtw { get; }
    public decimal PodiumInclBtw { get; }
    public decimal BtwPercentage { get; }
    public decimal DeelnemersExclBtw => ExclusiefBtw(DeelnemersInclBtw);
    public decimal PodiumExclBtw => ExclusiefBtw(PodiumInclBtw);
    public decimal TotaalInclBtw => DeelnemersInclBtw + PodiumInclBtw;
    public decimal Btw => TotaalInclBtw - DeelnemersExclBtw - PodiumExclBtw;

    private decimal ExclusiefBtw(decimal bedrag) =>
        decimal.Round(bedrag / (1 + BtwPercentage / 100), 2, MidpointRounding.AwayFromZero);
}
