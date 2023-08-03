function EMGmod = EMGmodification(EMGDATA,WindowSize,modoption)

switch modoption
    case 'average'
        tempBuf = averageEMG(EMGDATA,WindowSize);
        %
    case 'integrate'
        tempBuf = integrateEMG(EMGDATA,WindowSize);
        %
    otherwise
        disp('Wrong option number... Data will be averaged');
        tempBuf = averageEMG(EMGDATA,WindowSize);
        %average
end

EMGmod = tempBuf;

end

%---------------- subfunctions -----------------
function Buf = averageEMG(EMGDATA,WindowSize)
n = length(EMGDATA(1,:));
K = floor(n/WindowSize);

for i=1:K
    Buf(:,i) = abs(mean(EMGDATA(:,WindowSize*(i-1)+1:WindowSize*i),2));  
end
    
end


function Buf = integrateEMG(EMGDATA,WindowSize)
        
n = length(EMGDATA(1,:));
K = floor(n/WindowSize);

for i=1:K
    Buf(:,i) = abs(sum(EMGDATA(:,WindowSize*(i-1)+1:WindowSize*i),2));    
end
    
end





%-------------------------------------------------------