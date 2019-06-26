# Fortnite-Trainer

Due to the size of the files, I was not able to upload the whole source and so this is only the code, and not the design. The full source is avaialable here: https://1drv.ms/u/s!AqlzLccwyBBsg9R1ydCaviP_wt4QCA?e=bNFPNz

*IMPORTANT* If it crashes straight away, make sure the "GreyBackground" image and "tessdata" is in the same folder as the program, and the tessarct OCR is installer (Install-Package Tesseract from th nuget package manager).

Simply change the x and y values until the image (saved as RecentCapture.png in the local directory) is around the health bar. My settings have the hud scaled to 1.25 scale but I dont know if this is neccesary, I just changed it so it would be eaasier for the text to read.
Adjust the "frequencyOfCheck" to however often you want it to check the health.
Change the "comPort" to your arduino's comport.
