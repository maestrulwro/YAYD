# YAYD
YAYD - Yet Another Youtube Downloader - GUI for youtube-dl
I wanted to post it when I finished it. But I don't think I will finish it in the near future. So here it goes.
It should work if the user doesn't force it. So, please don't force it.

After compiling with Visual Studio, YAYD.exe should be located in the same folder as youtube-dl.exe and ffmpeg.exe. Please download and rename these command line programs yourself. Links:
https://ytdl-org.github.io/youtube-dl/index.html
https://www.ffmpeg.org/

I am currently reviewing the code and commenting it in order to make it easy for other people to understand what I did. Yes, the code is probably very inefficient (cringe?), but at least I tried. Fix it and make a pull request if you can do better, thank you!

Why would you need Yet Another Youtube Downloader?
Sites that are free require you to download link by link. The length of the download is limited (30 minutes). And also you have no control over the quality of the download.
Locally installed programs either contain questionable code or are paid. My aim is to make something free that can be trusted.

What's up with youtube-dl and FFmpeg?
Youtube-dl is another open source project, creating a command line based Python program able to get info and download audio&video from different sites.
FFmpeg is a open-source, command line based audio&video converter, used to convert the web specific formats to MP3's.
Both those programs are required to run YAYD; interactions between them is done through command line interface.

Why didn't I integrate the functionality of youtube-dl and FFmpeg into YAYD, if they are also open source?
I have no legal background and directly using code from those projects would have raised some problems. Also, I don't know how to combine multiple programming languages into one executable. Moreover, youtube-dl is frequently updated, and using its source code inside YAYD's would mean that either someone would have to recompile YAYD every time youtube-dl is updated, or YAYD would remain outdated.
Ideally, YAYD could be abandoned for years, and if youtube-dl and FFmpeg CLI syntax remains the same, YAYD could still be used.

Legal trouble with Youtube?
Well, I personally don't use YAYD, i listen music directly from Youtube. I did this for fun mostly. I AM NOT RESPONSIBLE FOR THE WAY YOU USE THIS. And if I am asked to take this down for legal reasons, I will. Contact me!

What's up with the NBT library?
I planned to enable music library functionality. Ideally, YAYD would parse the titles of the videos downloaded or would contact 3rd party meta databases to get ID3v2 tags for the final MP3 files. Guess I am good at planning, but not at doing.
And yes, it is the same format Minecraft uses. And yes, I made it myself. And yes, you can use it too in your projects (the same License applies to it, though I might change that in the future).

If you discovered this, give me a sign so that I know I didn't do this for nothing.

May the ( mass x acceleration ) be with you!
