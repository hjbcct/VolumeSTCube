import pandas as pd
import os
from tqdm import *
from tqdm import tqdm

# Run only once to merge the example data

HERE = os.path.dirname(__file__)
locationDataPath = os.path.join(HERE, 'locations.json')
valueDataPath = os.path.join(HERE, 'timeseriesdata.json')

locationData = pd.read_json(locationDataPath)
valueData = pd.read_json(valueDataPath)

dataMergedFilePath = os.path.join(HERE, 'data_merged')
if not os.path.exists(dataMergedFilePath):
    os.makedirs(dataMergedFilePath)

data = pd.DataFrame(locationData).drop(['x','y','order'],axis=1)

for index in tqdm(range(8472)):
    def mapValue(series, index):
            return valueData[index:index+1].loc[:,[int(series['rid'])]].values[0][0]
    data['val'] = data.apply(mapValue,args=[index],axis=1)
    data.to_csv(f'{dataMergedFilePath}/LOC_AQI_{index}.csv',index=None)