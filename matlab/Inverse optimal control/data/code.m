%%%%%%%%%%%%%%%%%%%%%%% Data Index %%%%%%%%%%%%%%%%%%%%%%%
%   1 : tx              11 : dtheta_y         
%   2 : x               12 : ddtheta_x        
%   3 : y               13 : ddtheta_y        
%   4 : dx              14 : Activation Curve1
%   5 : dy              15 : Activation Curve2
%   6 : ddx             
%   7 : ddy             
%   8 : theta_x         
%   9 : theta_y      
%  10 : dtheta_x      
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
for i = 1:20
    fileName = sprintf("Data%d.mat", i);
    load(fileName);
end