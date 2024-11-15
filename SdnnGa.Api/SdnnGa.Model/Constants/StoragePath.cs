﻿namespace SdnnGa.Model.Constants;

public static class StoragePath
{
    public const string BaseDirectory = "v1";
    public const string ModelPath = BaseDirectory + "/session-{0}/epoch-{1}/model_configs/model-{2}.json"; 
    public const string XDataPath = BaseDirectory + "/session-{0}/data/x_data.csv";
    public const string YDataPath = BaseDirectory + "/session-{0}/data/y_data.csv";
}
