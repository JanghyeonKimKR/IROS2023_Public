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