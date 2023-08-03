% This function is called by IOCsolver_IROS2023

function A = CalAmat(ss,aa)

global ts    
m = 2; n = 8; o = 8;
    A11 = zeros(n,m); A12 = zeros(n,o); A13 = eye(n); A14 = zeros(n,n);
    A12 = zeros(m,m); A22 = zeros(m,o); A23 = zeros(m,n); A24 = zeros(m,n);

    %%- A matrix subparts Calculation
    % A11: itself
    A12 = 2*eye(n) .* ss';
    % A13: itself
    A14 = Get_df_ds_mat(ss); % or A14 = GetA14(ss);
    A21 = eye(2) .* aa';
    % A22: itself
    % A23: itself
    A24 = [0, 0, 0, 1,   0, 0, 0, 0;
           0, 0, 0, 0,   0, 0, 0, 1 ];

    %%- Combine all subparts
    A = ts*[ A11, A12, A13, A14;
          A21, A22, A23, A24 ];

end