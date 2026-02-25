1. [x] Remove concept of time from the weatherStation. Treat the station like a db
2. [x] Implement Windspeed, as seperate list...
    1. [x] All that extra stuff....
3. [x] All station logic placed in WeatherMap
4. [x] use actual data
    1. [ ] Tune weather color min/max to match data
    2. [ ] Calculate min max for color
5. [x] Implement to show data from ANY mouse location, this requires either doing calculation or embeding data into each pixel when we build the field. Done through recalc. Lerping a single point is not expensive
- [x] Isobar
- [x] Switch to HSV color


questions after adding data:
- Can we do much longer loggings? For example 30 minutes of time, maybe get a weather reading every 5 seconds?
- What is the windspeed data in? Because currently is kinda extreme, either being around 2k or 50k
    - We need to preprocess the data? so the data is smoother
- Distance between devices



Alternate color idea:
- rgb

Then use pressure to median/chunk areas? So it kinda looks like a topological map

Or like add lines like isobars, but for like 10% of the pressure change?


Meeting
- We made a weather station.
- Very code heavy experiment
- Invent a science question: Does structral interference of a building cause differences in X
- Getting wind data file as time:KPM (do the calc during the logging step)
- Physical device picture
- Possibly make pyplots for weather as well as visual display (I can do this, I Carson.)
- Thinking of limitations: small space, outside/inside, VME error, time recoding differences, recording length, device is fragile(there is now glue), only 1 device 
- From the past presentations, If we can show the code WITHOUT showing the code. i.e., a diagram so we are not just showing blocks of text. FLOWCHARTS
- From the past presentations, we can draw the device wiring, instead of a photo. So its more clear... MSPAINT/drawings
- No thrumpy slides
- Make the presentation fun
- Discusse HAL prob function?!?! -- How do sensors work. Around the apparatus slide
- Explain the VMe sensor ?!?!?!
- End slides with probing people with possible discussion topics to help people think of questions to ask
- Think of possible applications. Like in farming/arg stuff.
- Mentioned that we joined groups.
- Thinking of ideas for experiment 3, starting with a scientific question using fact that we have acess to a pseudo weather station. 
- Practice on tuesday