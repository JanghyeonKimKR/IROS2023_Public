% This function is called by CalAmat

function A14 = GetA14(ss)
    
    mb = 1.057 * 1e-3; rb = 3.175 * 1e-3;
    Jb = 4.262 * 1e-9; % (2/5)*mb*rb^2 
    mp = 1.000 * 1e-4;
    lx = 35.73 * 1e-3; ly = 51.14 * 1e-3;
    Jpx = 1.0639e-08; % (1/12)*mp*lx^2
    Jpy = 8.7177e-08; % (1/3)*mp*ly^2
    g = 9.79641;

    %% - For LeftTop elements
    %% - w.r.t ss(1)    
    Apart = (mb*ss(1)^2+Jpx)^(-1); 
    Bpart = -2*mb*ss(1)*ss(2)*ss(4) - (mb*g*ss(1)+mp*g*lx/2)*cos(deg2rad(ss(3)));
    dApart = (-1/(mb*ss(1)^2 + Jpx)^2) * 1*mb*ss(1);
    dBpart = -2*mb*ss(2)*ss(4) - mb*g*cos(deg2rad(ss(3)));
    df4_ds1 = dApart * Bpart + Apart * dBpart;

    %% - w.r.t ss(2)    
    df4_ds2 = (mb*ss(1)^2+Jpx)^(-1)*(-2*mb*ss(1)*ss(4));

    %% - w.r.t ss(3)    
    df4_ds3 = (mb*ss(1)^2+Jpx)^(-1) * ( (mb*g*ss(1) + mp*g*lx/2)*sin(deg2rad(ss(3))) );

    %% - w.r.t ss(4)    
    df4_ds4 = (mb*ss(1)^2+Jpx)^(-1) * (-2*mb*ss(1)*ss(2)); 

    A14LeftTop = [ 0,  (1/(Jb/rb^2)+mb)*mb*ss(4)^2,                   0,  df4_ds1;
                   1,   0,                                            0,  df4_ds2;
                   0,  (1/(Jb/rb^2)+mb)*(-mb)*g*cos(deg2rad(ss(3))),  0,  df4_ds3;
                   0,  (1/(Jb/rb^2)+mb)*mb*ss(1)*2*ss(4),             1,  df4_ds4 ];

    
    %% - For RightBottom elements
    %% - w.r.t ss(1)    
    Apart = (mb*ss(5)^2+Jpy)^(-1); 
    Bpart = -2*mb*ss(5)*ss(6)*ss(8) - (mb*g*ss(5)+mp*g*ly/2)*cos(deg2rad(ss(7)));
    dApart = (-1/(mb*ss(5)^2 + Jpy)^2) * 1*mb*ss(5);
    dBpart = -2*mb*ss(6)*ss(8) - mb*g*cos(deg2rad(ss(7)));
    df8_ds5 = dApart * Bpart + Apart * dBpart;

    %% - w.r.t ss(2)    
    df8_ds6 = (mb*ss(5)^2+Jpy)^(-1)*(-2*mb*ss(5)*ss(8));

    %% - w.r.t ss(3)    
    df8_ds7 = (mb*ss(5)^2+Jpy)^(-1) * ( (mb*g*ss(5) + mp*g*ly/2)*sin(deg2rad(ss(7))) );

    %% - w.r.t ss(4)    
    df8_ds8 = (mb*ss(5)^2+Jpy)^(-1) * (-2*mb*ss(5)*ss(6)); 
    
    A14RightBottom = [ 0,  (1/(Jb/rb^2)+mb)*mb*ss(8)^2,                   0,  df8_ds5;
                       1,   0,                                            0,  df8_ds6;
                       0,  (1/(Jb/rb^2)+mb)*(-mb)*g*cos(deg2rad(ss(7))),  0,  df8_ds7;
                       0,  (1/(Jb/rb^2)+mb)*mb*ss(5)*2*ss(8),             1,  df8_ds8 ];

    A14 = -eye(8)+ [ A14LeftTop, zeros(4,4) ;
                     zeros(4,4), A14RightBottom ];

end