namespace SdnnGa.Model.Constants;

public static class JobSettings
{
    public static class CreateModel
    {
        public const string ModelRangeConfigSettingName = "model-range-config";
        public const string ModelConfigNetPathSettingName = "model-range-dotnet-path";
        public const string SessionIdSettingName = "session-id";
        public const string EpocheNoSettingName = "epoche-no";
        public const string ModelIdSettingName = "model-id";
    }

    public static class FitModel
    {
        public const string XTrainPathSettingName = "x-train";
        public const string YTrainPathSettingName = "y-train";
        public const string UseEarlyStoppingSettingName = "use-early-stopping";
        public const string MinDeltaSettingName = "min-delta";
        public const string PatienceSettingName = "patience";
        public const string IsLearnWithValidationSettingName = "is-learn-with-validation";
        public const string OptimizerSettingName = "optimizer";
        public const string LossFuncSettingName = "loss-func";
        public const string EpochsSettingName = "epochs";
        public const string BatchSizeSettingName = "batch-size";
        public const string ModelIdSettingName = "model-id";
        public const string SessionIdSettingName = "session-id";
        public const string EpocheNoSettingName = "epoche-no";
    }

    public static class GeneticEpoche
    {
        public const string SessionIdSettingName = "session-id";
        public const string ModelRangeConfigSettingName = "model-range-config";
    }
}
