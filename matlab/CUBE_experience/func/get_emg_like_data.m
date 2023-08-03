function emgData = get_emg_like_data(emg,numCh,numCheckCh, PLOT)
% clear; clc; close all;
Fs = 50;
[bh, ah] = butter(4, 1*2/Fs, 'high');
emgData = (-1)*filtfilt(bh, ah, emg(:, 1:numCh));

%- figure 1
if PLOT
    figure(); 
    nSubplot = numCheckCh;
    for i=1:nSubplot
        subplot(nSubplot,1,i);
        plot(emg(:, i)); hold on;
        plot(emgData(:, i));
        xlim([1, length(emg(:,i))]);
        ylim([-255, 255]);
    end
end

% end