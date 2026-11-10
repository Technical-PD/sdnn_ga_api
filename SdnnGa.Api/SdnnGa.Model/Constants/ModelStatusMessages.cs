namespace SdnnGa.Model.Constants;

/// <summary>
/// Службові значення NeuralNetworkModel.FitError (UI розпізнає їх, щоб показати окремий статус).
/// </summary>
public static class ModelStatusMessages
{
    public const string StoppedByUser = "Stopped by user.";

    public const string Interrupted = "Interrupted: the application was closed before this model finished training.";
}
