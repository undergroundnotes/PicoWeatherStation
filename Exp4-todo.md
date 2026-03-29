Steps needed to make this a live weather map
1. [x] Unify data type
2. Wind speed changes node size. This needs to be done at the end, because I removed everything needed to easily test this. This frees up a color channel, so its easier to visualizer
3. Figure out how to mock?!?!?!. This is bs
4. Server reading
5. 3 point interpolation
6. Do something smart with value min/maxs


Server reading:
- server end point is json. So json parsing into my data type
    - I will need to edit the datatype to semi match the formatting??

Ok. An issue I have is we once read at 60fps. This is not good enough. I want to always know the data changes.
Here is how I will do it: on a seperate thread do the server reading which fills a(2 or 3) list for each station; the update loop checks this list at 60fps.

Inject a reference to the list through program.cs, having the seperate thread outside of LiveGame.cs

LiveGame would need to track the "read count" and compare it each frame to the reference count. If the counts dont match load the readings.

What does this list look like?????
1. Either a multi D array of pre made objects read
2. or A single list of the json object

probably 2

There is not internal step

---

switch to using date time, instead of float time.

Potentially remove time setter from everything. This is either removing time completely, or switching to datetime

---
todo:
0. [x] Detach wind from colors
1. [x] 3 stations
2. [x] The rest
3. [ ] Isobars


Beauty:
- [x] Bg image of winnipeg
- [ ] ~~green colors are potentially invisable~~
- [x] Station icon something other than a circle
- [ ] something with wind
- [ ] click and drag station locations?!?!
    - [ ] FIELD space coords
