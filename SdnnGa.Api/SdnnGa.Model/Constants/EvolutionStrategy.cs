namespace SdnnGa.Model.Constants;

/// <summary>
/// Як створюються моделі наступного покоління.
/// </summary>
public static class EvolutionStrategy
{
    /// <summary>Мутації найкращої моделі попереднього покоління (поведінка за замовчуванням).</summary>
    public const string Mutation = "Mutation";

    /// <summary>Схрещування двох батьківських моделей.</summary>
    public const string Crossover = "Crossover";

    /// <summary>Схрещування двох батьків, після чого дитина ще й мутує.</summary>
    public const string CrossoverAndMutation = "CrossoverAndMutation";

    public static readonly string[] All = { Mutation, Crossover, CrossoverAndMutation };

    public static bool UsesCrossover(string strategy) => strategy == Crossover || strategy == CrossoverAndMutation;
}

/// <summary>
/// Спосіб схрещування прихованих шарів двох батьків.
/// </summary>
public static class CrossoverMethod
{
    /// <summary>Шари до випадкової точки розрізу — від батька A, решта — від батька B.</summary>
    public const string SinglePoint = "SinglePoint";

    /// <summary>Кожен прихований шар береться від випадково обраного батька.</summary>
    public const string Uniform = "Uniform";

    /// <summary>Кількість нейронів кожного шару — випадкова суміш розмірів обох батьків.</summary>
    public const string Blend = "Blend";

    public static readonly string[] All = { SinglePoint, Uniform, Blend };
}

/// <summary>
/// Як була отримана конкретна модель (зберігається в NeuralNetworkModel.Origin).
/// </summary>
public static class ModelOrigin
{
    public const string Random = "Random";
    public const string Mutation = "Mutation";
    public const string Crossover = "Crossover";
    public const string CrossoverAndMutation = "CrossoverAndMutation";

    public static string FromStrategy(string strategy) => strategy switch
    {
        EvolutionStrategy.Crossover => Crossover,
        EvolutionStrategy.CrossoverAndMutation => CrossoverAndMutation,
        _ => Mutation,
    };
}
