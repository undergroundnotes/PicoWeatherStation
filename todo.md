1. [x] Remove concept of time from the weatherStation. Treat the station like a db
2. [x] Implement Windspeed, as seperate list...
    1. [x] All that extra stuff....
3. [x] All station logic placed in WeatherMap
4. [x] use actual data
    1. [ ] Tune weather color min/max to match data
    2. [ ] Calculate min max for color
5. [x] Implement to show data from ANY mouse location, this requires either doing calculation or embeding data into each pixel when we build the field. Done through recalc. Lerping a single point is not expensive


questions after adding data:
- Can we do much longer loggings? For example 30 minutes of time, maybe get a weather reading every 5 seconds?
- What is the windspeed data in? Because currently is kinda extreme, either being around 2k or 50k
    - We need to preprocess the data? so the data is smoother



Alternate color idea:
- rgb

Then use pressure to median/chunk areas? So it kinda looks like a topological map

Or like add lines like isobars, but for like 10% of the pressure change?