function [W, C]=ExtractMusclesynergy(EMG, numSyn, maxIter, myeps)
    global numCh  numSam numRe 
    global gap
    
    [numCh, numSam, numRe] = size(EMG);
    %W0 = rand(numCh, numSyn);
    %W0 = 0.1*ones(numCh,numSyn);
    %C0 = 0.1*ones(numSyn, numSam, numRe);
    W0 = 5 - gap/2 + (gap) .* rand(numCh,numSyn);
    C0 = 5 - gap/2 + (gap) .* rand(numSyn, numSam, numRe);
    options=optimset('LargeScale','off','Display','off', 'MaxFunEvals', 30000, 'MaxIter', 5000);
    
    error = CostFun(W0, C0, EMG);
    disp([ 'gap : ', num2str(gap),' // numSyn : ' ,num2str(numSyn),' // count : ', num2str(0) ,' // loss : ', num2str(error)]);
    for cnt = 1:maxIter
        pre_error = error;
        
        % compute Wstar, Cstar
        [Wstar,~] = fmincon(@(W) CostFun(W, C0, EMG),W0,[],[],[],[],myeps*ones(numCh, numSyn), ones(numCh, numSyn),@NONLCON,options);
        Wstar = Wstar./max(Wstar);
        W0 = Wstar;
        
        [Cstar,~] = fmincon(@(C) CostFun(W0, C, EMG),C0,[],[],[],[],myeps*ones(numSyn, numSam, numRe), ones(numSyn, numSam, numRe), @NONLCON, options);
        % Cstar = Cstar ./ max(max(Cstar));
        C0 = Cstar;

        disp([ 'gap : ', num2str(gap),' // numSyn : ' ,num2str(numSyn),' // count : ', num2str(cnt) ,' // loss : ', num2str(error), ' // VAF : ', num2str(VAF(EMG,W0*C0))]);
        % compute error
        error = CostFun(W0, C0, EMG);
        if abs(pre_error - error) < myeps
            break;
        end
    end
    
    W = W0;
    C = C0;
end

%%
function J=CostFun(W, C, EMG)
global numCh numSam numRe

EMGhat = W*C;

% J = mean(abs(EMG-EMGhat)./EMG, 'all');


error = EMG - EMGhat;
J = 0;
for i = 1:numRe
    tmp = reshape(error(:,:,i), 1, numCh*numSam);
    J = J + tmp*tmp';
end
end

function [g,h]=NONLCON(W,~,~)
% inequality constraints
g=[

];
% equality constraints
h=[ 
   
];
end