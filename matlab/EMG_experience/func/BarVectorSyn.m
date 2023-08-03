function MS_fig = BarVectorSyn(W, C, task_index)
    [~, numW] = size(W);
    [~,time] = size(C);
    MS_fig = figure('Position',[1 41 1920 962]);

    % Muscle Synergy
    subplot(numW+1,numW,1)
    for i = 1:numW
        subplot(numW+1,numW,numW*numW + i)
        b = barh(flip(W(:,i)));
        yticklabels(flip({'ch1','ch2','ch3','ch4','ch5','ch6','ch7','ch8'}))
        set(gca,'FontSize',14, "XLim", [0 1])
        b.FaceColor = 'flat';

    end
    
    % Activation Cureve
    for i = 1:numW
        subplot(numW+1,numW, [numW*(i-1)+1, numW*(i-1)+numW])
        plot(C(i,:), 'b', 'LineWidth', 3); hold on;
        %plot((ones(1,100).*task_index)', (linspace(0, 1, 100).*ones(4,1))','k', 'LineWidth',2);
    end
    
end