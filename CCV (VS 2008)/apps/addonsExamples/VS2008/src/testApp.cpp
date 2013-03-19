#include "testApp.h"
#include "stdio.h"
#include "ofUtils.h"
#include "Filters/Filters.h"
#include <string>
//--------------------------------------------------------------
void testApp::setup()
{
	ccv = new ofxNCoreVision;
	raCamWidth = ccv->camWidth;
	raCamHeight = ccv->camHeight;
	raSourceImg.allocate(raCamWidth, raCamHeight);
	raProcessedImg.allocate(raCamWidth, raCamHeight);
	raHeightImg = cvCreateImage(cvGetSize(raProcessedImg.getCvImage()), 8, 1);

	raSendEnable = false;
	startHeightmap = false;
	startBlobs = false;
	startContour = false;
	startGraymap = false;
	startSimpleGrid = false;
	startCompleteGrid = false;
	raFrameseq = 0;
	frameseq = 0;
	//setup communication
	raLocalHost = "127.0.0.1";
	raPort = 3333;

	raSocket.setup(raLocalHost, raPort);
	raHighestCount = 0;
	raBlobCount = 0;
	raContourSize = 0;
	raTotalSize = 0;
	//must be an even number
	raPathLength = 32;

	countHeight = 0;
	countWidth = 0;
}


/******************************************************************************
* The update function runs continuously. Use it to update states and variables
*****************************************************************************/
void testApp::update(){
	frameseq += 1;
	/* HEIGHTMAP */
	if(startHeightmap)
	{
		raProcessedImg = ccv->filter->ampImg;
		raSourceImg = ccv->filter->grayImg; //ccv->sourceImg;
		//stretch
		raSourceImg.blurGaussian(7);
		raSourceImg.contrastStretch();

		raIPLsource = raSourceImg.getCvImage();
		raIPLprocessed = raProcessedImg.getCvImage();

		raHeightMap.clear();
		raNormalizedMap.clear();
		raReducedMap.clear();
		
		raHeightMap = generateHeightMap(raIPLsource, raIPLprocessed, raHeightImg);
		//raNormalizedMap = normalizeHeightMap(raHeightMap);
		raReducedMap = reduceHeightMap(raHeightMap, 40);
		raProcessedImg = raHeightImg;

		if(raSendEnable)
		{
			//messages
			ofxOscBundle raBundle;
			ofxOscMessage raSet;
			
			//data msg
			raSet.setAddress( "/raCom/test" );
			raSet.addStringArg("set");
			raSet.addIntArg(raReducedMap.size() * 3); //length
			
			int fixSize = raReducedMap.size();
			//vertice values (x y z)
			for( int j=0; j< fixSize; j++ ) {
				//ccv->tracker.calibrate->cameraToScreenPosition(raReducedMap[j].x, raReducedMap[j].y);
				raSet.addFloatArg(raReducedMap[j].x); //flipping axis for unity
				raSet.addFloatArg(raReducedMap[j].height / 255);
				raSet.addFloatArg(raReducedMap[j].y);
			}
			//packing and sending
			raBundle.addMessage( raSet ); //add message to bundle
			raSocket.sendBundle( raBundle ); //send bundle
		}
	}

	/* REGULAR BLOBS */
	if(startBlobs)
	{
		if(raSendEnable)
			sendBlobs();		
	}

	/* BLOBS WITH CONTOURS */
	if(startContour)
	{
		if(raSendEnable)
			sendBlobs();
		
		if(ccv->bShowInterface)
		{
			raBlobCount = 0;
			raContourSize = 0;
			raTotalSize = 0;
			std::map<int, Blob> raBlobs;
			raBlobs = ccv->getBlobs(); //get blobs from tracker
			vector<ofPoint> raPts;
			if(raBlobs.size() > 0)
			{
				raBlobCount = raBlobs.size();
				map<int, Blob>::iterator raBlob;
				for(raBlob = raBlobs.begin(); raBlob != raBlobs.end(); raBlob++)
				{
					raPts.clear();
					raPts = raBlob->second.pts;
					raContourSize += raPts.size();
				}
				raTotalSize = (raContourSize * 2) + (raBlobCount * 6);
			}
		}
	}
	
	/* NEW HEIGHTMAP based on contours and raster */
	//step 1 - contour
	//step 2 - resampling contour
	//step 3 - based on contour, create raster
	//step 4 - smaller patches in raster for high variations
	//step 5 - create heightmap based on raster patches
	//step 6 - send data*
	if(startGraymap)
	{
		raSourceImg = ccv->filter->grayImg;//ccv->sourceImg;
		raIPLsource = raSourceImg.getCvImage();
		raHeightMap.clear();
		bndBoxes.clear();
		outlines.clear();

		std::map<int, Blob> blobs;
		blobs = ccv->getBlobs(); //get blobs from tracker
		if(blobs.size() > 0)
		{
			map<int, Blob>::iterator blob;
			for(blob = blobs.begin(); blob != blobs.end(); blob++)
			{
				vector<ofPoint> contourPnts;
				contourPnts = blob->second.pts;
				if(contourPnts.size() > 0)
				{
					Path2D contourPath;
					for(int i = 0; i < contourPnts.size(); i++)
					{
						Point2D onePoint(contourPnts[i].x, contourPnts[i].y);
						contourPath.push_back(onePoint);
					}
					Path2D resampledPath = resample(contourPath);
					Box2D bndBox = getBoundingBox(resampledPath);
					bndBox.widthstep = 20;
					bndBoxes.push_back(bndBox);
					outlines.push_back(resampledPath);
					//first raster is 20x20 px
					for(int i = bndBox.y; i <= bndBox.y+bndBox.height; i+=bndBox.widthstep)
					{
						for(int j = bndBox.x; j <= bndBox.x+bndBox.width; j+=bndBox.widthstep)
						{
							//check every patch in raster for variance
							Point2D current = Point2D(j, i);
							float variance = getPixelVariance(raIPLsource, current, bndBox.widthstep, bndBox.widthstep);
							//std::cout << variance << std::endl;
							//if there is not much variation store the average pixel as depth (with inside polygon test)
							if(variance <= 5)
							{
								float pxlDepth = getPixelDepth(raIPLsource, current, bndBox.widthstep, bndBox.widthstep, resampledPath);
								if(pxlDepth > 0)
								{
									HeightMap singlePixel;
									singlePixel.x = current.x + (bndBox.widthstep/2);
									singlePixel.y = current.y + (bndBox.widthstep/2);
									singlePixel.height = pxlDepth;
									raHeightMap.push_back(singlePixel);
								}
							}
							if(variance > 5) 
							{
								Box2D patch;
								patch.x = current.x;
								patch.y = current.y;
								patch.width = bndBox.widthstep;
								patch.height = bndBox.widthstep;
								patch.widthstep = (bndBox.widthstep/2);
								bndBoxes.push_back(patch);
								//double grid
								for(int id = patch.y; id <= patch.y+patch.height; id+=patch.widthstep)
								{
									for(int jd = patch.x; jd <= patch.x+patch.width; jd+=patch.widthstep)
									{
										Point2D cpnt = Point2D(jd, id);
										if(variance < 10)
										{
											float pxlDepth = getPixelDepth(raIPLsource, cpnt, patch.widthstep, patch.widthstep, resampledPath);
											if(pxlDepth > 0)
											{
												HeightMap singlePixel;
												singlePixel.x = cpnt.x + (patch.widthstep/2);
												singlePixel.y = cpnt.y + (patch.widthstep/2);
												singlePixel.height = pxlDepth;
												raHeightMap.push_back(singlePixel);
											}
										}else{
											//4x grid
											Box2D subpatch;
											subpatch.x = patch.x;
											subpatch.y = patch.y;
											subpatch.width = patch.widthstep;
											subpatch.height = patch.widthstep;
											subpatch.widthstep = (patch.widthstep/2);
											bndBoxes.push_back(subpatch);
											for(int ie = subpatch.y; ie <= subpatch.y+subpatch.height; ie+=subpatch.widthstep)
											{
												for(int je = subpatch.x; je <= subpatch.x+subpatch.width; je+=subpatch.widthstep)
												{
													Point2D subcpnt = Point2D(je, ie);
													float pxlDepth = getPixelDepth(raIPLsource, subcpnt, subpatch.widthstep, subpatch.widthstep, resampledPath);
													if(pxlDepth > 0)
													{
														HeightMap singlePixel;
														singlePixel.x = subcpnt.x + (subpatch.widthstep/2);
														singlePixel.y = subcpnt.y + (subpatch.widthstep/2);
														singlePixel.height = pxlDepth;
														raHeightMap.push_back(singlePixel);
													}
												}
											}
										}
									}
								}
							}
						}
					}

					if(raSendEnable)
					{
						sendHeightMap();
					}
				}
			}
		}
	}

	if(startSimpleGrid)
	{
		raSourceImg = ccv->filter->grayImg;//ccv->sourceImg;
		//stretch
		//raSourceImg.blurGaussian(7);
		raSourceImg += 20;

		raIPLsource = raSourceImg.getCvImage();
		raHeightMap.clear();
		bndBoxes.clear();
		outlines.clear();
		raBlobHeightMap.clear();

		std::map<int, Blob> blobs;
		blobs = ccv->getBlobs(); //get blobs from tracker
		if(blobs.size() > 0)
		{
			map<int, Blob>::iterator blob;
			for(blob = blobs.begin(); blob != blobs.end(); blob++)
			{
				vector<ofPoint> contourPnts;
				contourPnts = blob->second.pts;
				if(contourPnts.size() > 0)
				{
					HeightData _heightdata;
					_heightdata.ID = blob->second.id;
					Path2D contourPath;
					for(int i = 0; i < contourPnts.size(); i++)
					{
						Point2D onePoint(contourPnts[i].x, contourPnts[i].y);
						contourPath.push_back(onePoint);
					}
					Path2D resampledPath = resample(contourPath);
					Box2D bndBox = getBoundingBox(resampledPath);
					bndBox.widthstep = 4;
					bndBoxes.push_back(bndBox);
					outlines.push_back(resampledPath);

					countHeight = 0;
					countWidth = 0;
					vector<float> _heights;

					float xCoM = 0;
					float yCoM = 0;

					//raster
					for(int i = bndBox.y-3; i <= bndBox.y+bndBox.height+2; i+=bndBox.widthstep)
					{
						for(int j = bndBox.x-3; j <= bndBox.x+bndBox.width+2; j+=bndBox.widthstep)
						{
							//naive approach
							Point2D current = Point2D(j, i);
							float pxlDepth = getPixelDepth(raIPLsource, current, bndBox.widthstep, bndBox.widthstep, resampledPath);
							HeightMap singlePixel;
							singlePixel.x = current.x;
							xCoM += current.x;
							singlePixel.y = current.y;
							yCoM += current.y;
							singlePixel.height = pxlDepth;
							_heights.push_back(pxlDepth);
							raHeightMap.push_back(singlePixel);
							countWidth++;
						}
						countHeight++;
					}
					xCoM /= raHeightMap.size();
					yCoM /= raHeightMap.size();
					bndBox.x = xCoM;
					bndBox.y = yCoM;
					countWidth /= countHeight;
					bndBox.width = countWidth;
					bndBox.height = countHeight;
					_heightdata.bbox = bndBox;
					_heightdata.heightPoints = _heights;
					raBlobHeightMap.push_back(_heightdata);
				}
			}
		}

		//send
		if(raSendEnable)
		{
			sendHeightBlobs();
		}
	}
	
	if(startCompleteGrid)
	{
		raSourceImg = ccv->filter->grayImg;//ccv->sourceImg;
		raIPLsource = raSourceImg.getCvImage();
		raProcessedImg = ccv->filter->ampImg;
		raIPLprocessed = raProcessedImg.getCvImage();

		raHeightMap.clear();
		
		raHeightMap = gridMap(raIPLsource, raIPLprocessed);
		countWidth = 64;
		countHeight = 48;

		//for every blob seperate message
		if(raSendEnable)
		{
			sendHeightMap();
		}
	}
}

/******************************************************************************
* The draw function paints the textures onto the screen. It runs after update.
*****************************************************************************/
void testApp::draw(){

//////////////////////////////////////////////////////////////////
//This is only visual output and can be disabled by pressing space
//////////////////////////////////////////////////////////////////
 if(ccv->bShowInterface)
 {
	//instructions
	ofSetColor(0xFFFFFF);
	ofDrawBitmapString("Instructions", 725, 610);
	ofDrawBitmapString("------------", 725, 620);
	ofDrawBitmapString("1) start Height Map", 725, 630);
	ofDrawBitmapString("2) start Regular blob", 725, 640);
	ofDrawBitmapString("3) start Contour Map", 725, 650);
	ofDrawBitmapString("4) start new Heightmap", 725, 660);
	ofDrawBitmapString("5) start simple grid", 725, 670);
	ofDrawBitmapString("6) start UDP com", 725, 680);
	ofDrawBitmapString("space) mini mode", 725, 690);
	ofDrawBitmapString("esc) QUIT", 725, 700);
 
	/* HEIGHTMAP */
	if(startHeightmap)
	{
		int hmLength = raReducedMap.size() * 3;
		if(hmLength > raHighestCount)
		{
			raHighestCount = hmLength;
		}
		std::stringstream hmLengthString;
		std::stringstream hmHighestString;
		hmLengthString << hmLength;
		hmHighestString << raHighestCount;
		raFrameseq += 1;

		//interface Look
		ofSetColor(255, 0, 0);
		ofFill();
		ofRect(0,615, 720, 270);
		ofSetColor(0, 0, 0);
		ofFill();
		ofRect(360,640, 320, 240);
		ofSetColor(0xFFFFFF);
		if(raSendEnable)
		{
			ofDrawBitmapString("Rasmus test module- Send Enabled -current vertice count: "+ hmLengthString.str() + " High: " + hmHighestString.str(), 0, 610);
		}else{
			ofDrawBitmapString("Rasmus test module- Send Disabled -current vertice count: "+ hmLengthString.str() + " High: " + hmHighestString.str(), 0, 610);
		}
		ofDrawBitmapString("EXTRACTED SOURCE", 120, 635);
		raProcessedImg.draw(20, 640); 	
		ofDrawBitmapString("HEIGHTMAP", 480, 635);
		glPushMatrix();
		glTranslatef( 360, 640, 0.0 );
		glScalef( 1, 1, 0.0 );
		//ofSetColor(0x00FFFF);
		ofFill();

		for( int j=0; j<raReducedMap.size(); j++ ) {
			ofSetColor(raReducedMap[j].height, 0, 255-raReducedMap[j].height);
			ofRect( raReducedMap[j].x * raCamWidth, raReducedMap[j].y * raCamHeight, 15, 15 );
			//std::cout << raReducedMap[j].height << std::endl;
		}
			
		/* Draw contour
		std::map<int, Blob> blobs;
		std::map<int, Blob>::iterator iter;
		blobs = ccv->getBlobs(); //get blobs from tracker
		for(iter=blobs.begin(); iter!=blobs.end(); iter++)
		{
			Blob drawBlob;
			drawBlob = iter->second;
			drawBlob.drawContours();
		}
		*/
			
		glPopMatrix();	
	}
	/* REGULAR BLOBS */
	if(startBlobs)
	{
		if(raSendEnable)
		{
			ofDrawBitmapString("Rasmus test module- Regular blobs -Send Enabled", 0, 610);
		}else{
			ofDrawBitmapString("Rasmus test module- Regular blobs -Send Disabled", 0, 610);
		}	
	}
	/* BLOBS WITH CONTOURS */
	if(startContour)
	{
		std::stringstream ssBlobCount;
		std::stringstream ssContourSize;
		std::stringstream ssTotalSize;
		ssBlobCount << raBlobCount;
		ssContourSize << raContourSize;
		ssTotalSize << raTotalSize;
		ofDrawBitmapString("Blob count: " + ssBlobCount.str(), 400, 700);
		ofDrawBitmapString("Contour points count: " + ssContourSize.str(), 400, 715);
		ofDrawBitmapString("Total message size: " + ssTotalSize.str(), 400, 730);
		if(raSendEnable)
		{
			ofDrawBitmapString("Rasmus test module- Blobs with contour information -Send Enabled", 0, 610);
		}else{
			ofDrawBitmapString("Rasmus test module- Blobs with contour information -Send Disabled", 0, 610);
		}	
	}
	if(startGraymap)
	{
		raSourceImg.draw(380, 630);
		drawPath();
		glPushMatrix();
		glTranslatef( 20, 640, 0.0 );
		ofFill();
		for( int j=0; j<raHeightMap.size(); j++ ) {
			ofSetColor(raHeightMap[j].height, 0, 255-raHeightMap[j].height);
			ofRect( raHeightMap[j].x, raHeightMap[j].y, 1, 1 );
			//std::cout << raHeightMap[j].height << std::endl;
		}
		glPopMatrix();
	}
	if(startSimpleGrid)
	{
		raSourceImg.draw(380, 630);
		drawPath();
		glPushMatrix();
		glTranslatef( 20, 640, 0.0 );
		ofFill();
		for( int j=0; j<raHeightMap.size(); j++ ) {

			float height = raHeightMap[j].height * 5;

			if(height <= 255)
				ofSetColor(0, 0, height/3);
			if(height <= 510 && height > 255)
				ofSetColor(0, height/3, 255 );
			if(height <= 765 && height > 510)
				ofSetColor(0, 255,  255 - height/3);
			if(height <= 1020 && height > 765)
				ofSetColor(height / 3, 255,  0);
			if(height <= 1275 && height > 1020)
				ofSetColor(255, 255 - height / 3,  0);

			ofRect( raHeightMap[j].x, raHeightMap[j].y, 3, 3);
			//std::cout << raHeightMap[j].height << std::endl;
		}
		glPopMatrix();
	}

	if(startCompleteGrid)
	{
		glPushMatrix();
		glTranslatef( 20, 640, 0.0 );
		ofFill();
		for( int j=0; j<raHeightMap.size(); j++ ) {
			ofSetColor(raHeightMap[j].height, 0, 255-raHeightMap[j].height);
			ofRect( raHeightMap[j].x, raHeightMap[j].y, 1, 1 );
			//std::cout << raHeightMap[j].height << std::endl;
		}
		glPopMatrix();
	}
 }
}

/*****************************************************************************
* KEY EVENTS
*****************************************************************************/
void testApp::keyPressed  (int key){
	switch (key)
	{
		case '1':
			if(startHeightmap)
			{
				startHeightmap = false;
			}else{
				startBlobs = false;
				startContour = false;
				startGraymap = false;
				startSimpleGrid = false;
				startCompleteGrid = false;
				startHeightmap = true;
			}

		break;

		case '2':
			if(startBlobs)
			{
				startBlobs = false;
			}else{				
				startContour = false;
				startHeightmap = false;
				startGraymap = false;
				startSimpleGrid = false;
				startCompleteGrid = false;
				startBlobs = true;
			}

		break;

		case '3':
			if(startContour)
			{
				startContour = false;
			}else{
				startBlobs = false;
				startHeightmap = false;
				startGraymap = false;
				startSimpleGrid = false;
				startCompleteGrid = false;
				startContour = true;
			}

		break;

		case '4':
			if(startGraymap)
			{
				startGraymap = false;
			}else{
				startBlobs = false;
				startHeightmap = false;
				startContour = false;
				startSimpleGrid = false;
				startCompleteGrid = false;
				startGraymap = true;
			}

		break;

		case '5':
			if(startSimpleGrid)
			{
				startSimpleGrid = false;
			}else{
				startBlobs = false;
				startHeightmap = false;
				startContour = false;
				startGraymap = false;
				startCompleteGrid = false;
				startSimpleGrid = true;
			}

		break;

		case '6':
			if(raSendEnable)
			{
				raSendEnable = false;
			}else{
				raSendEnable = true;
			}

		break;

		case '7':
			if(startCompleteGrid)
			{
				startCompleteGrid = false;
			}else{
				startBlobs = false;
				startHeightmap = false;
				startContour = false;
				startGraymap = false;
				startSimpleGrid = false;
				startCompleteGrid = true;
			}

		break;
	}
}

//--------------------------------------------------------------
void testApp::keyReleased  (int key){

}

/*****************************************************************************
*	MOUSE EVENTS
*****************************************************************************/
void testApp::mouseMoved(int x, int y ){
}

//--------------------------------------------------------------
void testApp::mouseDragged(int x, int y, int button){
}

//--------------------------------------------------------------
void testApp::mousePressed(int x, int y, int button){

}

//--------------------------------------------------------------
void testApp::mouseReleased(){

}

/*****************************************************************************
 *	TOUCH EVENTS
 *****************************************************************************/
void testApp::TouchDown( Blob b)
{
}

void testApp::TouchUp( Blob b)
{	
}

void testApp::TouchMoved( Blob b)
{
}
/*****************************************************************************
 *	Heightmaps
 *****************************************************************************/
float testApp::getPixelDepth(IplImage *sourceImg, Point2D startPnt ,int width, int height, Path2D polygon)
{
	float sourcePixel = 0;
	int cnt = 0;
	int fheight = startPnt.y + height;
	int fwidth = startPnt.x + width;

	Point2D inside = startPnt;
	inside.x += (width / 2);
	inside.y += (height / 2);	
	int m = InsidePolygon(polygon, polygon.size()-1, inside, 2);
	
	if(m > 0)
	{
		for(int a = startPnt.y; a < fheight; a++)
		{
			for(int b = startPnt.x; b < fwidth; b++)
			{	
				CvPoint pt = {b,a};
				uchar* sourceFrame = &((uchar*)(sourceImg->imageData + sourceImg->widthStep*pt.y))[pt.x];			
				if(sourceFrame[0] > sourcePixel)
				{
					sourcePixel += sourceFrame[0];
					cnt++;
				}
			}
		}
	}

	if(sourcePixel != 0)
		sourcePixel /= cnt;

    return sourcePixel;
}

float testApp::getPixelVariance(IplImage *sourceImg, Point2D startPnt ,int width, int height)
{
	float mean = 0;
	int cnt = 0;
	int fheight = startPnt.y + height;
	int fwidth = startPnt.x + width;
	CompHeightMap genMap;
	HeightMap singlePixel;
	//mean
	for(int a = startPnt.y; a < fheight; a++)
	{
		for(int b = startPnt.x; b < fwidth; b++)
		{	
			CvPoint pt = {b,a};
			uchar* sourceFrame = &((uchar*)(sourceImg->imageData + sourceImg->widthStep*pt.y))[pt.x];			
			mean += sourceFrame[0];
			singlePixel.x = b;
			singlePixel.y = a;
			singlePixel.height = sourceFrame[0];
			genMap.push_back(singlePixel);
			cnt++;
		}
	}
	//std deviation
	float avrg = 0;
	if(mean != 0)
	{
		mean /= cnt; //mean
		cnt = 0;
		for(int c = 0; c < genMap.size(); c++)
		{
			float difference = genMap[c].height - mean;
			difference *= difference; //(squared)
			avrg += difference;
			cnt++;
		}	
		avrg /= cnt;
		avrg = sqrt(avrg);
	}

    return avrg;
}


CompHeightMap testApp::generateHeightMap(IplImage *sourceImg, IplImage *processedImg, IplImage *heightImg) 
{
	CompHeightMap genMap;
	HeightMap singlePixel;

	for(float a = 0; a < processedImg->height; a++)
	{
		for(float b = 0; b < processedImg->width; b++)
		{	
			
			CvPoint pt = {b,a};
			uchar* processedFrame = &((uchar*)(processedImg->imageData + processedImg->widthStep*pt.y))[pt.x];	
            uchar* sourceFrame = &((uchar*)(sourceImg->imageData + sourceImg->widthStep*pt.y))[pt.x];  
			uchar* heightFrame = &((uchar*)(heightImg->imageData + heightImg->widthStep*pt.y))[pt.x];
           
			heightFrame[0] = 0;

			int processedPixel = processedFrame[0];  
			float sourcePixel = sourceFrame[0];
			
			if(processedPixel < 256 && processedPixel > 10)
			{
				singlePixel.x = b / raCamWidth;
				singlePixel.y = a / raCamHeight;
				singlePixel.height = sourcePixel; //check this colorvalue normalization
				genMap.push_back(singlePixel);
				heightFrame[0] = sourcePixel;
			}
		}
	}

	return genMap;
}

CompHeightMap testApp::gridMap(IplImage *sourceImg, IplImage *processedImg) 
{
	CompHeightMap genMap;
	HeightMap singlePixel;

	for(float a = 0; a < processedImg->height; a+=5)
	{
		for(float b = 0; b < processedImg->width; b+=5)
		{	
			
			CvPoint pt = {b,a};
			uchar* processedFrame = &((uchar*)(processedImg->imageData + processedImg->widthStep*pt.y))[pt.x];
			uchar* sourceFrame = &((uchar*)(sourceImg->imageData + sourceImg->widthStep*pt.y))[pt.x];  
			
			float sourcePixel = sourceFrame[0];
			int processedPixel = processedFrame[0];  
			
			if(processedPixel < 256 && processedPixel > 10)
			{
				singlePixel.x = b;
				singlePixel.y = a;
				singlePixel.height = sourcePixel;
				genMap.push_back(singlePixel);
			}else{
				singlePixel.x = b;
				singlePixel.y = a;
				singlePixel.height = 0;
				genMap.push_back(singlePixel);
			}
		}
	}

	return genMap;
}

CompHeightMap testApp::normalizeHeightMap(CompHeightMap inputMap) 
{
	//contrast stretching / normalization
	// c = lowest pixel value - d = highest
	// a = lower limit - b = higher limit
	// Pout = (Pin - c) ((b-a)/(d-c)) + a
	CompHeightMap returnMap;
	HeightMap singlePixel;
	HeightMap bounds = getBoundPixelValue(inputMap);
	float c = bounds.x;
	float d = bounds.y;
	//avoid deviding by 0 error;
	if(d-c == 0)
	{
		c = 0;
		d = 255;
	}
	float a = 0;
	float b = 255;

	for(int j=0; j < inputMap.size(); j++)
	{
		singlePixel.x = inputMap[j].x;
		singlePixel.y = inputMap[j].y;
		singlePixel.height = (inputMap[j].height - c) * ((b-a)/(d-c)) + a;
		returnMap.push_back(singlePixel);
	}


	return inputMap;
}

CompHeightMap testApp::reduceHeightMap(CompHeightMap inputMap, int reduceCount)
{	
	
	CompHeightMap returnMap;
	CompHeightMap averageMap;
	HeightMap singlePixel;

	for(int j=0; j < inputMap.size(); j++)
	{
		singlePixel.x = inputMap[j].x;
		singlePixel.y = inputMap[j].y;
		singlePixel.height = inputMap[j].height;
		averageMap.push_back(singlePixel);
		if(averageMap.size() == reduceCount)
		{
			HeightMap avrgPixel;
			for(int i=0; i<averageMap.size(); i++)
			{
				avrgPixel.height += averageMap[i].height;
			}
			avrgPixel.x = averageMap[averageMap.size() / 2].x; //median
			avrgPixel.y = averageMap[averageMap.size() / 2].y; //median
			avrgPixel.height = averageMap[averageMap.size() / 2].height; //median
			returnMap.push_back(avrgPixel);
			averageMap.clear();
		}	
	}
	return returnMap;
}

HeightMap testApp::getBoundPixelValue(CompHeightMap inputMap)
{
	HeightMap singlePixel;
	//init
	singlePixel.x = 255;
	singlePixel.y = 0;
	
	for(int j=0; j < inputMap.size(); j++)
	{
		if(inputMap[j].height < singlePixel.x)
		{
			singlePixel.x = inputMap[j].height;
		}
		if(inputMap[j].height > singlePixel.y)
		{
			singlePixel.y = inputMap[j].height;
		}
	}

	return singlePixel;
}

Box2D testApp::getBoundingBox(Path2D inputPath)
{
	Point2D minPixel;
	Point2D maxPixel;
	//init
	minPixel.x = 999999999.0f;
	minPixel.y = 999999999.0f;
	maxPixel.x = 0.0f;
	maxPixel.y = 0.0f;
	
	for(int j=0; j < inputPath.size(); j++)
	{
		if(inputPath[j].x < minPixel.x)
			minPixel.x = inputPath[j].x;
		if(inputPath[j].y < minPixel.y)
			minPixel.y = inputPath[j].y;
		if(inputPath[j].x > maxPixel.x)
			maxPixel.x = inputPath[j].x;
		if(inputPath[j].y > maxPixel.y)
			maxPixel.y = inputPath[j].y;	
	}
	
	Point2D width1 = Point2D(minPixel.x, minPixel.y);
	Point2D width2 = Point2D(maxPixel.x, minPixel.y);
	float width = getDistance(width1, width2);
	Point2D height1 = Point2D(minPixel.x, minPixel.y);
	Point2D height2 = Point2D(minPixel.x, maxPixel.y);
	float height = getDistance(height1, height2);
	Box2D bb = Box2D(minPixel.x, minPixel.y, width, height);

	return bb;
}

void testApp::sendBlobs()
{

	std::map<int, Blob> blobs;
	blobs = ccv->getBlobs(); //get blobs from tracker

	ofxOscBundle b;

	if(blobs.size() == 0)
	{
		//Sends alive message - saying 'Hey, there's no alive blobs'
		ofxOscMessage alive;
		
		// TUIO v1.0
		alive.setAddress("/tuio/2Dcur");
		// TUIO v1.1
		//alive.setAddress("/tuio/2Dcur source ccv@localhost");

		alive.addStringArg("alive");

		//Send fseq message
		ofxOscMessage fseq;

		fseq.setAddress( "/tuio/2Dcur" );

		fseq.addStringArg( "fseq" );
		fseq.addIntArg(frameseq);

		b.addMessage( alive ); //add message to bundle
		b.addMessage( fseq ); //add message to bundle
		raSocket.sendBundle( b ); //send bundle
	}
	else //actually send the blobs
	{
		map<int, Blob>::iterator this_blob;
		for(this_blob = blobs.begin(); this_blob != blobs.end(); this_blob++)
		{
			//Set Message
			ofxOscMessage set;

			set.setAddress( "/tuio/2Dcur" );
			set.addStringArg("set");
			set.addIntArg(this_blob->second.id); //id
			set.addFloatArg(this_blob->second.centroid.x);  // x
			set.addFloatArg(this_blob->second.centroid.y); // y
			set.addFloatArg(this_blob->second.D.x); //dX
			set.addFloatArg(this_blob->second.D.y); //dY
			set.addFloatArg(this_blob->second.maccel); //m
			//set.addFloatArg(this_blob->second.boundingRect.width); // wd
			//set.addFloatArg(this_blob->second.boundingRect.height);// ht
			b.addMessage( set ); //add message to bundle
			
			//Sends contour information along with the rest
			if(startContour)
			{
				vector<ofPoint> raPts = this_blob->second.pts;
				if(raPts.size() > 0)
				{
					ofxOscMessage cntr;
					cntr.setAddress( "/tuio/2Dcur" );
					cntr.addStringArg("contour");
					cntr.addIntArg(this_blob->second.id); //id
					cntr.addIntArg(raPts.size() * 2);
					for(int i = 0; i < raPts.size(); i++)
					{
						ccv->tracker.calibrate->cameraToScreenSpace(raPts[i].x, raPts[i].y);
						cntr.addFloatArg(raPts[i].x);
						cntr.addFloatArg(raPts[i].y);
					}
					b.addMessage( cntr );
				}
			}
		}

		//Send alive message of all alive IDs
		ofxOscMessage alive;
		alive.setAddress("/tuio/2Dcur");
		alive.addStringArg("alive");

		std::map<int, Blob>::iterator this_blobID;
		for(this_blobID = blobs.begin(); this_blobID != blobs.end(); this_blobID++)
		{
			alive.addIntArg(this_blobID->second.id); //Get list of ALL active IDs
		}

		//Send fseq message
		ofxOscMessage fseq;
		fseq.setAddress( "/tuio/2Dcur" );
		fseq.addStringArg( "fseq" );
		fseq.addIntArg(frameseq);

		b.addMessage( alive ); //add message to bundle
		b.addMessage( fseq ); //add message to bundle

		raSocket.sendBundle( b ); //send bundle
	}

}

void testApp::sendHeightBlobs()
{

	std::map<int, Blob> blobs;
	blobs = ccv->getBlobs(); //get blobs from tracker

	ofxOscBundle b;

	if(blobs.size() == 0)
	{
		//Sends alive message - saying 'Hey, there's no alive blobs'
		ofxOscMessage alive;
		
		// TUIO v1.0
		alive.setAddress("/tuio/2Dcur");
		// TUIO v1.1
		//alive.setAddress("/tuio/2Dcur source ccv@localhost");

		alive.addStringArg("alive");

		//Send fseq message
		ofxOscMessage fseq;

		fseq.setAddress( "/tuio/2Dcur" );

		fseq.addStringArg( "fseq" );
		fseq.addIntArg(frameseq);

		b.addMessage( alive ); //add message to bundle
		b.addMessage( fseq ); //add message to bundle
		raSocket.sendBundle( b ); //send bundle
	}
	else //actually send the blobs
	{
		map<int, Blob>::iterator this_blob;
		for(this_blob = blobs.begin(); this_blob != blobs.end(); this_blob++)
		{
			//Set Message
			ofxOscMessage set;

			set.setAddress( "/tuio/2Dcur" );
			set.addStringArg("set");
			set.addIntArg(this_blob->second.id); //id
			set.addFloatArg(this_blob->second.centroid.x);  // x
			set.addFloatArg(this_blob->second.centroid.y); // y
			set.addFloatArg(this_blob->second.D.x); //dX
			set.addFloatArg(this_blob->second.D.y); //dY
			set.addFloatArg(this_blob->second.maccel); //m
			//set.addFloatArg(this_blob->second.boundingRect.width); // wd
			//set.addFloatArg(this_blob->second.boundingRect.height);// ht
			b.addMessage( set ); //add message to bundle
			
			//Sends height information along with the rest
			if(startSimpleGrid)
			{
				if(raBlobHeightMap.size() > 0)
				{
					ofxOscMessage cntr;
					cntr.setAddress( "/tuio/2Dcur" );
					cntr.addStringArg("height");
					cntr.addIntArg(this_blob->second.id); //id
					for(int i = 0; i < raBlobHeightMap.size(); i++)
					{
						if(raBlobHeightMap[i].ID == this_blob->second.id)
						{
							ccv->tracker.calibrate->cameraToScreenSpace(raBlobHeightMap[i].bbox.x, raBlobHeightMap[i].bbox.y);
							cntr.addFloatArg(raBlobHeightMap[i].bbox.x);
							cntr.addFloatArg(raBlobHeightMap[i].bbox.y);
							//cntr.addFloatArg(this_blob->second.centroid.x);  // x
							//cntr.addFloatArg(this_blob->second.centroid.y); // y
							cntr.addIntArg(raBlobHeightMap[i].bbox.width);
							cntr.addIntArg(raBlobHeightMap[i].bbox.height);
							cntr.addIntArg(raBlobHeightMap[i].heightPoints.size());
							for(int c=0; c<raBlobHeightMap[i].heightPoints.size(); c++)
							{
								cntr.addFloatArg(raBlobHeightMap[i].heightPoints[c]);
							}
						}
					}
					b.addMessage( cntr );
				}
			}
		}

		//Send alive message of all alive IDs
		ofxOscMessage alive;
		alive.setAddress("/tuio/2Dcur");
		alive.addStringArg("alive");

		std::map<int, Blob>::iterator this_blobID;
		for(this_blobID = blobs.begin(); this_blobID != blobs.end(); this_blobID++)
		{
			alive.addIntArg(this_blobID->second.id); //Get list of ALL active IDs
		}

		//Send fseq message
		ofxOscMessage fseq;
		fseq.setAddress( "/tuio/2Dcur" );
		fseq.addStringArg( "fseq" );
		fseq.addIntArg(frameseq);

		b.addMessage( alive ); //add message to bundle
		b.addMessage( fseq ); //add message to bundle

		raSocket.sendBundle( b ); //send bundle
	}

}


void testApp::sendHeightMap()
{
	//messages
	ofxOscBundle raBundle;
	ofxOscMessage raSet;
	
	//data msg
	raSet.setAddress( "/raCom/test" );
	raSet.addStringArg("set");
	raSet.addIntArg(raHeightMap.size()); //length
	raSet.addIntArg(countWidth); //length
	raSet.addIntArg(countHeight); //length
	int fixSize = raHeightMap.size();
	//std::cout << fixSize << std::endl;
	//vertice values (x y z)
	for( int j=0; j< fixSize; j++ ) {
		//ccv->tracker.calibrate->cameraToScreenSpace(raHeightMap[j].x, raHeightMap[j].y);
		//raSet.addFloatArg(raHeightMap[j].x / raCamWidth); //flipping axis for unity
		raSet.addFloatArg(raHeightMap[j].height / 255);
		//raSet.addFloatArg(raHeightMap[j].y / raCamHeight);
	}
	//packing and sending
	raBundle.addMessage( raSet ); //add message to bundle
	raSocket.sendBundle( raBundle ); //send bundle
}
float testApp::getDistance(Point2D p1, Point2D p2)
{
	float dx = p2.x - p1.x;
	float dy = p2.y - p1.y;
	float distance = sqrt((dx * dx) + (dy * dy));
	
	return distance;
}

float testApp::pathLength(Path2D inputPath)
{
		float distance = 0;
		
		for (int i = 1; i < (int)inputPath.size(); i++)
		{
			distance += getDistance(inputPath[i - 1], inputPath[i]);
		}

		return distance;
}

Path2D testApp::resample(Path2D inputPath) 
{

	// step a + b)
	float I = pathLength(inputPath) / (raPathLength - 1);
	
	float D = 0.0;
	Path2D newPath;
	
	//store the first starting point, as we not resample it
	newPath.push_back(inputPath.front());
	//step c)
	for(int x = 1; x < (int)inputPath.size(); x++)
	{
		Point2D currentPoint  = inputPath[x];
		Point2D previousPoint = inputPath[x-1];
		float d = getDistance(previousPoint, currentPoint);
			

		if ((D + d) >= I)
		{
			float qx = previousPoint.x + ((I - D) / d) * (currentPoint.x - previousPoint.x);
			float qy = previousPoint.y + ((I - D) / d) * (currentPoint.y - previousPoint.y);
			
			Point2D newPoint(qx, qy);

			newPath.push_back(newPoint);

			inputPath.insert(inputPath.begin() + x, newPoint);

			D = 0.0;
		}

		else D += d;
	
	}
	//rounding errors
	if (newPath.size() == (raPathLength - 1))
	{
		newPath.push_back(inputPath.back());
	}
	
	return newPath;
}

void testApp::drawPath()
{
	
	glPushMatrix();
	glTranslatef( 380, 630, 0.0 );
	glScalef( 1, 1, 0.0 );
	//box grid
	ofSetColor(120, 120, 120);
	ofNoFill();
	ofRect(0, 0, 320, 240);
	//contour
	ofSetColor(255, 255, 255);
	ofFill();
	for(int f = 0; f < outlines.size(); f++)
	{
		Path2D inputPath = outlines[f];
		for(int i = 0; i < inputPath.size(); i++)
		{
			Point2D currentPoint  = inputPath[i];
			ofRect( currentPoint.x, currentPoint.y, 2, 2 );
			if(i+1 < inputPath.size())
			{
				Point2D targetPoint  = inputPath[i+1];
				ofLine(currentPoint.x, currentPoint.y, targetPoint.x, targetPoint.y);
			}
		}
	}
	ofNoFill();
	//ofSetColor(0, 255, 0);
	//ofRect( bndBox.x, bndBox.y, bndBox.width, bndBox.height );
	 
	//grids
	ofSetColor(0, 0, 255);

	for(int s = 0; s < bndBoxes.size(); s++)
	{
		Box2D bndBox=bndBoxes[s];
		for(int i = 0; i <= bndBox.width; i+=bndBox.widthstep)
		{
			for(int j = 0; j <= bndBox.height; j+=bndBox.widthstep)
			{
				ofLine(i+bndBox.x, bndBox.y, i+bndBox.x, bndBox.y+bndBox.height);
				ofLine(bndBox.x, j+bndBox.y, bndBox.x+bndBox.width, j+bndBox.y);
			}
		}
	}
	

	glPopMatrix();	

}

//point inside polygon test
int testApp::InsidePolygon(Path2D polygon, int N, Point2D p, int bound)
{
    //cross points count of x
    int __count = 0;

    //neighbour bound vertices
    Point2D p1, p2;

    //left vertex
    p1 = polygon[0];

    //check all rays
    for(int i = 1; i <= N; ++i)
    {
        //point is an vertex
        if(p == p1) return bound;

        //right vertex
        p2 = polygon[i % N];

        //ray is outside of our interests
        if(p.y < PMIN(p1.y, p2.y) || p.y > PMAX(p1.y, p2.y))
        {
            //next ray left point
            p1 = p2; continue;
        }

        //ray is crossing over by the algorithm (common part of)
        if(p.y > PMIN(p1.y, p2.y) && p.y < PMAX(p1.y, p2.y))
        {
            //x is before of ray
            if(p.x <= PMAX(p1.x, p2.x))
            {
                //overlies on a horizontal ray
                if(p1.y == p2.y && p.x >= PMIN(p1.x, p2.x)) return bound;

                //ray is vertical
                if(p1.x == p2.x)
                {
                    //overlies on a ray
                    if(p1.x == p.x) return bound;
                    //before ray
                    else ++__count;
                }

                //cross point on the left side
                else
                {
                    //cross point of x
                    double xinters = (p.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;

                    //overlies on a ray
                    if(fabs(p.x - xinters) < __DBL_EPSILON__) return bound;

                    //before ray
                    if(p.x < xinters) ++__count;
                }
            }
        }
        //special case when ray is crossing through the vertex
        else
        {
            //p crossing over p2
            if(p.y == p2.y && p.x <= p2.x)
            {
                //next vertex
                const Point2D& p3 = polygon[(i+1) % N];

                //p.y lies between p1.y & p3.y
                if(p.y >= PMIN(p1.y, p3.y) && p.y <= PMAX(p1.y, p3.y))
                {
                    ++__count;
                }
                else
                {
                    __count += 2;
                }
            }
        }

        //next ray left point
        p1 = p2;
    }

    //EVEN
    if(__count % 2 == 0) return(0);
    //ODD
    else return(1);
}