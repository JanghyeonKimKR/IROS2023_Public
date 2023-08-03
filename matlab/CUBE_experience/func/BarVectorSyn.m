function [] = BarVectorSyn(W, C)
    [~, numW] = size(W);
    [~,time] = size(C);
    figure();

    % Muscle Synergy
    subplot(numW+1,numW,1)
    for i = 1:numW
        subplot(numW+1,numW,numW*numW + i)
        b = barh(flip(W(:,i)));
        yticklabels(flip({'ch1','ch2','ch3','ch4','ch5','ch6','ch7','ch8'}))
        set(gca,'FontSize',14, "XLim", [0 1])
        b.FaceColor = 'flat';
%         % tricep
%         b.CData(2,:) = [1 1 0];
%         b.CData(3,:) = [1 1 0];
%         b.CData(4,:) = [1 1 0];
%         % bicep
%         b.CData(6,:) = [1 0 0];
%         b.CData(7,:) = [1 0 0];
%         b.CData(8,:) = [1 0 0];
    end
    
    % Activation Cureve
    for i = 1:numW
        subplot(numW+1,numW, [numW*(i-1)+1, numW*(i-1)+numW])
        plot(C(i,:), 'LineWidth', 2);
        axis([1 length(C) 0 1]);
    end
    
end