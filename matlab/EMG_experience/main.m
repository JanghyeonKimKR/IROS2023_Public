%%%%%%%%%%%%%%%%%%%%%%% Data Index %%%%%%%%%%%%%%%%%%%%%%%
%   1 : ts
%   2 : emg1
%   3 : emg2
%   4 : emg3
%   5 : emg4
%   6 : emg5
%   7 : emg6
%   8 : emg7
%   9 : emg8
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
close all;
clear -except numSyn gap file filesave
addpath('func\')
global numSyn gap file filesave
 
%% Data Processing(EMG Concatenation)
% concat emg
concat_emg = [];
task_index = [];
start_index = 10;

dt = 0.02;
for i = 1:40
    name = sprintf("data\\%s\\data%d.csv", file,i);
    data = readmatrix(name);

    % raw data
    emg = data(start_index:end-start_index,2:end);

    % data processing(emg)
    emgData = EMGProcessing(emg); % No normalize

    % concatp 
    concat_emg = [concat_emg, emgData];
    if mod(i,10) == 0
        task_index = [task_index; length(concat_emg)+1];
    end
end

[concat_emg, normalizer] = RetNormalizedData(concat_emg,8); % Normalize

% Plot Result
% emg & emg_hat plot
% EMG_fig = figure('Position',[1 41 1920 962]);
% for j = 1:8
%     subplot(8,1,j)
%     plot(concat_emg(j,:)', 'b', 'LineWidth',3); hold on
%     plot((ones(1,100).*task_index)', (linspace(0, 1, 100).*ones(4,1))','k', 'LineWidth',2);
% end

%% Synergy Extraction
[W, C] = ExtractMusclesynergy(concat_emg, numSyn, 20, 1e-7);

%% Plot Result
% emg & emg_hat plot
EMG_fig = figure('Position',[1 41 1920 962]);
concat_emg_hat = W*C;
for j = 1:8
    subplot(8,1,j)
    plot(concat_emg(j,:)', 'b', 'LineWidth',3); hold on
    %plot(concat_emg_hat(j,:)', 'r','LineWidth',3); hold on
    %plot((ones(1,100).*task_index)', (linspace(0, 1, 100).*ones(4,1))','k', 'LineWidth',2);
end
%synergy plot
MS_fig = BarVectorSyn(W, C, task_index);

%% Evaluation
loss = 0;
for i = 1:8
    vafs(i) = VAF(concat_emg(i,:)', concat_emg_hat(i,:)');
    loss = loss + norm(concat_emg(i,:)' - concat_emg_hat(i,:)');
end

%% Save
mkdir(sprintf('./result/%s', filesave));
saveas(EMG_fig, sprintf("./result/%s/EMG_fig%d.jpg", filesave, numSyn));
saveas(MS_fig, sprintf("./result/%s/MS_fig%d.jpg", filesave, numSyn));
save(sprintf('./result/%s/result_%d.mat', filesave, numSyn),'W','C','concat_emg','concat_emg_hat', 'loss', 'vafs', 'normalizer');