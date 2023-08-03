function processedEMG = EMGProcessing(emg)
    global filteredResult rmsResult numSam numCh
    [numSam,numCh] = size(emg);
    emgData = get_emg_like_data(emg,numCh,8,0);

    LPfiltered_EMG = zeros(numCh,numSam);
    movingRMS_EMG = zeros(numCh,numSam);
    %partition = 10;
    %figure()
    %tiledlayout(4,4)
    for i = 1:numCh
        %Voltage = (emgData(:,i)-mean(emgData(end-200:end,i)));
        Voltage = (emgData(:,i)-mean(emgData(:,i)));
        absVoltage = abs(Voltage);
        
        butterWorthLP(absVoltage,10);
        LPfiltered_EMG(i,:) = filteredResult;
        
        movingRMS(absVoltage,40);
        movingRMS_EMG(i,:) = rmsResult;

        %nexttile
        %plot(Voltage);
        %axis([1 length(Voltage) -100 100])
        %hf = plot(filteredResult,'g'); hold on;
        %hr = plot(rmsResult,'r'); hold off;
        %nexttile
    end

    WindowSize = 1;
    modEMG = EMGmodification(LPfiltered_EMG, WindowSize,'integrate');
    %norEMG = RetNormalizedData(modEMG,numCh);
    processedEMG = modEMG;
end

%------------------------- subfunctions --------------------------%

function butterWorthLP(data,freq)
global filteredResult

[B,A]=butter(4,2*freq/1000,'low');
% absdata = abs(data);
filteredResult = filtfilt(B,A,data);

end

function movingRMS(data, windowSize)
global rmsResult numSam

hWD = windowSize/2; % 10

for i=1:numSam
    
    if i < hWD
        x = data(1:i+hWD);
        y(i) = norm(x)/sqrt(length(x));
    else if i > numSam-hWD
             x = data(i-hWD:numSam);
             y(i) = norm(x)/sqrt(length(x));
        else % normal range
             x = data(i- hWD+1: i+hWD);
             y(i) = norm(x)/sqrt(length(x));   
         end
    end

rmsResult = y;     
end
end

    %% plot
%     for i = 1: numEMGre
%         emg = processedEMG(:,:,i);
%         if mod(i-1,12) == 0
%             figure(i)
%             tiledlayout(3,4);
%         end
%         nexttile
%         visualizeEMGDATA(emg, numEMGch, 5, 2)
%     end
    %% create DTW data
%     dist_list = [];
%     ref = processedEMG(1:2,30:80,40);
%     norEMG = [];
%     for i = 1:numEMGre
%         [dist, ix, iy] = dtw(ref, processedEMG(1:2,35:80,i));
%         dtwData = processedEMG(:,iy,i);
%         dist_list= [ dist_list dist ];
%     
% %         WindowSize = floor(length(iy)/20);
% %         modEMG = EMGmodification(dtwData, WindowSize,'integrate');
%         norEMG = retNormalizedData(dtwData(10:50),numEMGch);
%         dtwEMG(:,:,i) = norEMG;
       
%         - plot
%         if mod(i-1,12) == 0
%             figure(i)
%             tiledlayout(3,4);
%         end
%         nexttile
%         visualizeEMGDATA(norEMG, numEMGch, 5, 2)
%     end

%% -- plot all normalEMG & select normalEMG
%     % select shortest distance dtwData
%     k = find(dist_list < 3);
%     dtwEMG = dtwEMG(:,:,k);
%     % plot dtwData
%     for j = 1:k
%         w = dtwEMG(:,:,j);
%         if mod(j-1,12) == 0
%             figure(j)
%             tiledlayout(3,4);
%         end
%         nexttile
%         visualizeEMGDATA(w, numEMGch, 5, 2);
%     end