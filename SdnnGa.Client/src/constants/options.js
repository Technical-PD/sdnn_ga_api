export const OPTIMIZERS = [
  { value: 'adam', label: 'Adam', description: 'Adaptive learning rate. A reliable default.' },
  { value: 'sgd', label: 'SGD', description: 'Classic stochastic gradient descent. Slower, sometimes generalizes better.' }
]

export const LOSS_FUNCTIONS = [
  {
    value: 'categorical_crossentropy',
    label: 'Categorical cross-entropy',
    description: 'Multi-class classification with one-hot targets'
  },
  {
    value: 'binary_crossentropy',
    label: 'Binary cross-entropy',
    description: 'Yes/no or multi-label classification (0/1 targets)'
  },
  { value: 'mean_squared_error', label: 'Mean squared error', description: 'Regression' },
  { value: 'mean_absolute_error', label: 'Mean absolute error', description: 'Regression, less sensitive to outliers' },
  {
    value: 'mean_squared_logarithmic_error',
    label: 'Mean squared log error',
    description: 'Regression on values that span orders of magnitude'
  },
  {
    value: 'mean_absolute_percentage_error',
    label: 'Mean absolute percentage error',
    description: 'Regression measured in relative (%) error'
  }
]

export const ACTIVATIONS = [
  { value: 'relu', label: 'ReLU' },
  { value: 'sigmoid', label: 'Sigmoid' },
  { value: 'tanh', label: 'Tanh' },
  { value: 'softmax', label: 'Softmax' },
  { value: 'linear', label: 'Linear' }
]

export const SELECTION_CRITERIA = [
  { value: 'ByLoss', label: 'Lowest loss' },
  { value: 'ByAccuracy', label: 'Highest accuracy' }
]

export const labelOf = (options, value) => options.find(option => option.value === value)?.label ?? value ?? '—'

export const TRAINING_DEFAULTS = {
  lossFunc: 'categorical_crossentropy',
  fitMethod: 'adam',
  maxEpoches: 50
}

export const GENETIC_DEFAULTS = {
  maxEpoches: 10,
  countOfModelsInEpoch: 5,
  selectionCriterion: 'ByLoss',
  actFuncMutationProb: 20,
  countOfNeuronMutationProb: 30,
  countOfInternalLayerMutationProb: 20,
  biasMutationProb: 10,
  stopAccValue: 99,
  stopLossValue: 0.01
}

export const SEARCH_DEFAULTS = {
  countOfInternalLayers: 2,
  neurons: [4, 32],
  activationFunc: 'sigmoid',
  useBias: true
}

// Фіксовані параметри навчання, які задає сервер (GeneticEpochJob).
export const FIXED_TRAINING_SETTINGS = 'Batch size 50 · 25% of rows used for validation · early stopping after 10 epochs without improvement'
