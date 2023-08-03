%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% IOCsolver_IROS2023.m
% Purpose: Solve an Inverse Optimal Control problem with patient cube user
% data
% Author: Han U. Yoon and Janghyeon Kim (2023/02/27)
% Usage: Type IOCsolver_IROS2023 directly in the command window
% Remark:
% Revision: v1.0
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function IOCsolver_IROS2023

global nParam dimLambda ts

%%-- Load data
%-------------------------------- Column idx and Data-------- -------------------------
% 1:tx, 2:x, 3:y, 4:dx, 5:dy, 6:ddx, 7:ddy,    8:theta_x, 9:theta_y      
% 10 : dtheta_x, 11 : dtheta_y, 12 : ddtheta_x, 13 : ddtheta_y, 
% 14 : Activation Curve1, 15 : Activation Curve2, 16 : Activation Curve3, 
% 17 : Activation Curve4, 18 : Activation Curve5     
%--------------------------------------------------------------------------------------

ts = 20*1e-3;
% ts = 1

dNum = 1;
filename = sprintf('./data/Data%d',dNum);
S = load(filename);
[m ~] = size(S.Data(:,1));

%%-- Assign variables
x = S.Data(:,2); 
y = S.Data(:,3); 
dx = S.Data(:,4); 
dy = S.Data(:,5); 
ddx = S.Data(:,6); 
ddy = S.Data(:,7); 

theta_x = S.Data(:,8); 
theta_y = S.Data(:,9); 
dtheta_x = S.Data(:,10); 
dtheta_y = S.Data(:,11); 
ddtheta_x = S.Data(:,12); 
ddtheta_y = S.Data(:,13); 

act_curve1 = S.Data(:,14); 
act_curve2 = S.Data(:,15); 

%%-- Define a state - rowwise: time, colwise: all states at time k 
s = [ x, dx, theta_y, dtheta_y,     y, dy, theta_x, dtheta_x ];

%%- Define a control
a = [ act_curve2, act_curve1 ];

%%-- Initialize coefficient vector
nParam = 6;
%%- init method1
c_ax = 1/(max(act_curve1));
c_ay = 1/(max(act_curve2));
c_p = 1/(max(x)^2+max(y));
c_v = 1/(max(dx)^2+max(dy));
c_theta = 1/(max(theta_x)+max(theta_y));
c_omega = 1/(max(dtheta_x)+max(dtheta_y));
%%- init method2
% c_ax = 0.1*randn;
% c_ay = 0.1*randn;
% c_p = 0.1*randn;
% c_v = 0.1*randn;
% c_theta = 0.1*randn;
% c_omega = 0.1*randn;

%%-- Initialize z-vector
dimLambda = 8;
z0 = [c_ax, c_ay, c_p, c_v, c_theta, c_omega, 0.1*randn(1,dimLambda)];
options = optimset('LargeScale','off','Display','off');

%%- Solve IOC problem
c_star = fmincon(@cubeDyn,z0,[],[],[],[],[],[],@nonlcon,options,s,a)


end % end of function IOCsolver_IROS2023


%===============================================================================

%%-- objective function
function J = cubeDyn(z0,s,a)
    
   global nParam dimLambda ts

%    global A11 A12 A13 A14 A21 A22 A23 A24
   cost = 0;

   lindex = nParam + 1;
   l = z0(lindex:lindex+dimLambda-1)';
   ln = zeros(dimLambda,1);

   K = length(s);

   for k=1:K
      
      A = zeros(10,26);
      
      ss = s(k,:); aa = a(k,:);
      A = CalAmat(ss,aa);
      cvec = [z0(3:6), z0(3:6)]';
      ln = PropCostate(l,ss,cvec);

      z = [ [z0(1), z0(2)]';    [z0(3:6), z0(3:6)]';  l; ln ];

      cost = cost + sqrt((A*z)'*(A*z));
      l = ln;     

   end

J = cost / K

%  pause;

end

%===============================================================

function [g,h] = nonlcon(z0,~,~,~,~,~)

% size(z0)
normalval = 1;
Llimit = normalval*1e-4;
idx = 3; % fix할 인덱스

g = [
    -z0(1) + Llimit; % c_ax
    -z0(2) + Llimit; % c_ay
    -z0(3) + Llimit; % cp
    -z0(4) + Llimit; % cv
    -z0(5) + Llimit; % c_theta
    -z0(6) + Llimit; % c_omega
    -z0(7) + Llimit;
    -z0(8) + Llimit;
    -z0(9) + Llimit;
    -z0(10) + Llimit;
    -z0(11) + Llimit;
    -z0(12) + Llimit;
    -z0(13) + Llimit;
    -z0(14) + Llimit
];

h = [
    z0(idx) - normalval;
    %z0(6) - 1e-4;
    z0(1) - z0(2);
];
end



