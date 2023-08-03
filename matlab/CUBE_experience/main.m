%%%%%%%%%%%%%%%%%%%%%%% Data Index %%%%%%%%%%%%%%%%%%%%%%%
%   1 : tx              11 : dtheta_y           21 : emg2
%   2 : x               12 : ddtheta_x          22 : emg3
%   3 : y               13 : ddtheta_y          23 : emg4
%   4 : dx              14 : roll_mpu           24 : emg5
%   5 : dy              15 : pitch_mpu          25 : emg6
%   6 : ddx             16 : gx_mpu             26 : emg7
%   7 : ddy             17 : gy_mpu             27 : emg8    
%   8 : theta_x         18 : theta_x_c
%   9 : theta_y         19 : theta_y_c
%   10 : dtheta_x       20 : emg1
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
close all;clear; clc;
global rmsResult numSam
addpath('func\')

% Parameter
% C:\Users\kim\Desktop\IROS-2023\matlab\EMG_experience\result\sub3_0223\MS_fig3.jpg
% C:\Users\kim\Desktop\IROS-2023\matlab\EMG_experience\result\sub3_0223\result_3.mat
load(sprintf("result_emg/result.mat"))
numSyn = length(W(1,:));

% Each EMS's Muscle Synergy Extraction
start_index = 10;
subject = 3;
concatData = [];
concat_C = [];
concat_J = [];
for i = 1:20
    %
    Data = [];
    
    % [Start:End]
    name = sprintf("data\\sub%d\\data%02d.csv", subject, i);
    data = readmatrix(name);
    data = data(start_index + 1:end, :);

    % Raw Data
    emg = data(:, 20:end);

    % Data Processing(emg)
    emg = EMGProcessing(emg);
    emg = emg ./ normalizer;
    data(:,20:end)= emg';

    % Activation Curve Extraction
    [C, J] = ExtractActivation(emg, numSyn, 1e-7, W, 2);

    % single data
    ts = data(:, 1);
    x = data(:, 2);
    y = data(:, 3);
    dx = data(:, 4);
    dy = data(:, 5);
    ddx = data(:, 6);
    ddy = data(:, 7);
    theta_x = data(:, 8);
    theta_y = data(:, 9);
    dtheta_x = data(:, 10);
    dtheta_y = data(:, 11);
    ddtheta_x = data(:, 12);
    ddtheta_y = data(:, 13);
    emg = data(:, 20:end);

    % concat single data and save
    Data = [ts, x, y, dx, dy, ddx, ddy, theta_x, theta_y, dtheta_x, dtheta_y, ddtheta_x, ddtheta_y, C(2:3,:)'];
    save(sprintf("result/Data%d.mat", i), "Data");
    
    % moving averge
    numSam = length(dtheta_x);
    movingRMS(dx, 30)
    dx_MAF = rmsResult;
    movingRMS(dy, 30)
    dy_MAF = rmsResult;
    movingRMS(ddx, 30)
    ddx_MAF = rmsResult;
    movingRMS(ddy, 30)
    ddy_MAF = rmsResult;

    movingRMS(dtheta_x, 30)
    dtheta_x_MAF = rmsResult;
    movingRMS(dtheta_y, 30)
    dtheta_y_MAF = rmsResult;
    movingRMS(ddtheta_x, 30)
    ddtheta_x_MAF = rmsResult;
    movingRMS(ddtheta_y, 30)
    ddtheta_y_MAF = rmsResult;
     
    % Plot Data
    close
    fig = figure('Position', [1 41 1920 962]);
    subplot(6,2,1)
    plot(ts, x, ts, y, 'LineWidth',2);
    axis([min(ts), max(ts) -5 5])
    legend('x', 'y');
    subplot(6,2,3)
    plot(ts, dx_MAF, ts, dy_MAF, 'LineWidth',2);
    axis([min(ts), max(ts) -10 10])
    subplot(6,2,5)
    plot(ts, ddx_MAF, ts, ddy_MAF, 'LineWidth',2);
    axis([min(ts), max(ts) -30 30])
    subplot(6,2,7)
    plot(ts, theta_x, ts, theta_y, 'LineWidth',2);
    axis([min(ts), max(ts) -2.5 2.5])
    subplot(6,2,9)
    plot(ts, abs(dtheta_x_MAF), ts, abs(dtheta_y_MAF), 'LineWidth',2);
    axis([min(ts), max(ts) 0 20])
    subplot(6,2,11)
    plot(ts, abs(ddtheta_x_MAF), ts, abs(ddtheta_y_MAF), 'LineWidth',2);
    axis([min(ts), max(ts) 0 700])
    subplot(6,2,2)
    plot(ts, C(1,:), 'LineWidth',2)
    axis([min(ts), max(ts) 0 1])
    subplot(6,2,4)
    plot(ts, C(2,:), 'LineWidth',2)
    axis([min(ts), max(ts) 0 1])
    subplot(6,2,6)
    plot(ts, C(3,:), 'LineWidth',2)
    axis([min(ts), max(ts) 0 1])
    saveas(fig, sprintf("./result_cube/result%d.jpg", i));

    % Plot
    %BarVectorSyn(W, C);
    disp(i + " : Done!(" + J + ")")

    % Data Concatenation
    concatData = [concatData; data];
    concat_C = [concat_C; C'];
    concat_J = [concat_J; J];
end
% save concat data
save("result/concatData.mat", "concatData");

% single data
ts = concatData(:, 1);
x = concatData(:, 2);
y = concatData(:, 3);
dx = concatData(:, 4);
dy = concatData(:, 5);
ddx = concatData(:, 6);
ddy = concatData(:, 7);
theta_x = concatData(:, 8);
theta_y = concatData(:, 9);
dtheta_x = concatData(:, 10);
dtheta_y = concatData(:, 11);
ddtheta_x = concatData(:, 12);
ddtheta_y = concatData(:, 13);
emg = concatData(:, 20:end);

figure();
plot(x,'LineWidth',2); hold on;
plot(y, 'LineWidth',2);
legend('theta_x', 'theta_y')

%     figure();
% subplot(3,1,1)
% plot(theta_x,'LineWidth',2); hold on;
% plot(theta_y, 'LineWidth',2);
% legend('theta_x', 'theta_y')
% 
% subplot(3,1,2)
% plot(dtheta_x,'LineWidth',2); hold on;
% plot(dtheta_y, 'LineWidth',2);
% legend('dtheta_x', 'dtheta_y')
% 
% subplot(3,1,3)
% plot(ddtheta_x,'LineWidth',2); hold on;
% plot(ddtheta_y, 'LineWidth',2);
% legend('ddtheta_x', 'ddtheta_y')


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
