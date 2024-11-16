namespace SdnnGa.Model.Constants;

public static class JobSettings
{
    public static class CreateModel
    {
        public const string ModelConfigSettingName = "model_config";
        public const string ModelStoragePathSettingName = "model_storage_path";
    }

    public static class FitModelJob
    {
        public const string XTrainPathSettingName = "x_train";
        public const string YTrainPathSettingName = "y_train";
        public const string ModelConfigJsonPathSettingName = "model_config_json";
        public const string CountOfLinesSettingName = "count_of_lines";
        public const string CountOfInputsSettingName = "count_of_inputs";
        public const string CountOfOutputsSettingName = "count_of_outputs";
        public const string UseEarlyStoppingSettingName = "use_early_stopping";
        public const string MinDeltaSettingName = "min_delta";
        public const string PatienceSettingName = "patience";
        public const string IsLearnWithValidationSettingName = "is_learn_with_validation";
        public const string OptimizerSettingName = "optimizer";
        public const string LossFuncSettingName = "loss_func";
        public const string EpochsSettingName = "epochs";
        public const string BatchSizeSettingName = "batch_size";
        public const string WeightPathSettingName = "weight_path";
    }
}
