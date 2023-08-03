function [C, J]=ExtractActivation(EMG, numSyn, myeps, W0, gap)
    global numCh  numSam numRe 
    
    [numCh, numSam, numRe] = size(EMG);
    C0 = 5 - gap/2 + (gap) .* rand(numSyn, numSam, numRe);
    options=optimset('LargeScale','off','Display','off', 'MaxFunEvals', 15000, 'MaxIter', 1000);
    
    [C,~] = fmincon(@(C) CostFun(W0, C, EMG),C0,[],[],[],[],myeps*ones(numSyn, numSam, numRe), ones(numSyn, numSam, numRe), @NONLCON, options);
    J = CostFun(W0, C, EMG);
end

%%
function J=CostFun(W, C, EMG)
global numCh numSam numRe

for i = 1:numRe
    EMGhat(:,:,i) = W*C(:,:,i);
end

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