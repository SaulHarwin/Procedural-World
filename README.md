# Procedural World
 A procedurally generated terrain world.
 
 ### Perlin Noise
 ![Alt Text](https://rtouti.github.io/assets/images/perlin-noise-texture.png)
 <br />Perline noise is a function that looks like this. This can be used to make realistic terrain because it creates convinsing rolling hills. Then by layer different perlin noise layers up you can get even more realistic terrain that make all the terrain less smooth. 
 

 ### Climate 
To deside which biome each point should be in we need to workout the percipitation and Tempreture. 
|                        | Hottest                 | Hot                | Cold              | Coldest    |
| :-------------   | :-------------:        | :----------:     | :-----------:   | :-----------: |
| Dryest             | Desert                   | Savanna         | Boreal Forest | Artic
| Dry                 | Savanna                 | Grassland      | Boreal Forest | Tundra
| Wet                 | Tropical Rainforest | Grassland      | Boreal Forest | Tundra
| Wettest           | Tropical Rainforest | Forest            | Tundra          | Tundra
 #### Percipitation 
 If the earths surface was perfectly uniform, the long-term average rainfall would be uniform in distict latitudinal bands. However in the real world there are other factors such as winds that drive the clouds and the mountains which cause rain to fall one side of them or the other. However this would be extremely difficult to calculate so I will use a longitudinal map then I will add some random noise onto it to make it more realistic. 
 
 #### Temperature
 I want the temperarture to be based on the distance from the poles and equator and the height of the terrain. However I will add noise to this to amek it less uniform because in the real world it isn't so rigid. However, my game will be infinite so I will not have any poles or a center so I cannot do this. So I can make generate the temperature in uniform latitudinal bands and then add some random noise. 
 
 #### Combining Them
 I will then have to compare then percipitation and the temperature and pick out a biome from the table above. To prevent the biome pattern to be to uniform and checkaboardy I will have to play with the band width and how much randomness I apply to the temperature and heat maps. 
