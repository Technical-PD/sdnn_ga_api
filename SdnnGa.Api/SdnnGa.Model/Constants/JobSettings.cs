namespace SdnnGa.Model.Constants;

public static class JobSettings
{
    public static class CreateModel
    {
        public const string ModelConfigSettingName = "model_config";
        public const string SessionIdSettingName = "session-id";
        public const string EpocheNoSettingName = "epoche-no";
        public const string ModelIdSettingName = "model-id";
    }

    public static class FitModelJob
    {
        public const string XTrainPathSettingName = "x_train";
        public const string YTrainPathSettingName = "y_train";
        public const string UseEarlyStoppingSettingName = "use_early_stopping";
        public const string MinDeltaSettingName = "min_delta";
        public const string PatienceSettingName = "patience";
        public const string IsLearnWithValidationSettingName = "is_learn_with_validation";
        public const string OptimizerSettingName = "optimizer";
        public const string LossFuncSettingName = "loss_func";
        public const string EpochsSettingName = "epochs";
        public const string BatchSizeSettingName = "batch_size";
        public const string ModelIdSettingName = "model_id";
        public const string SessionIdSettingName = "session-id";
        public const string EpocheNoSettingName = "epoche-no";
    }
}
