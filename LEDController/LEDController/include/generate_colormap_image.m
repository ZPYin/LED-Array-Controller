mainPath = fileparts(fullfile(mfilename, 'fullpath'));
addpath(genpath(fullfile(mainPath, 'export_fig')));

clc;

nColor = 500;
colors = zeros(nColor, 3);
saturation = 0.5;
lightness = 0.5;

for iColor = 1:nColor
    colors(iColor, :) = hsl2rgb([(iColor - 1)/(nColor - 1) / 2 + 0.5, saturation, lightness]);
end

figure('position', [10, 10, 700, 400], 'Units', 'Pixels');
hold on;
for i = 1:nColor
    patch([0 1 1 0] + i, [0 0 1 1], colors(i, :), 'linest', 'none');
end
set(gcf, 'color', 'w')
set(gca, 'position', [0.1, 0.3, 0.48, 0.02], 'units', 'normalized');
axis off

export_fig(gcf, 'colorbar.png', '-r300');
hold off;