// src/setupHighcharts.js
import Highcharts from 'highcharts';

Highcharts.setOptions({
  chart: { backgroundColor: 'transparent' },
  title: { style: { color: '#EDEDED', fontWeight: 600, fontSize: '14px' } },
  xAxis: {
    gridLineColor: '#262626',
    lineColor: '#404040',
    labels: { style: { color: '#CFCFCF' } },
    title: { style: { color: '#CFCFCF' } }
  },
  yAxis: {
    gridLineColor: '#262626',
    labels: { style: { color: '#CFCFCF' } },
    title: { style: { color: '#CFCFCF' } }
  },
  legend: {
    itemStyle: { color: '#E6E6E6' },
    itemHoverStyle: { color: '#1DB954' }
  },
  tooltip: {
    backgroundColor: '#1b1b1b',
    borderColor: '#333',
    style: { color: '#EDEDED' }
  },
  credits: { enabled: false },
  colors: ['#1DB954','#58A6FF','#F1A33C','#FF5C8A','#C792EA','#4DD0E1','#FFD166','#7CDB67']
});

// Defensive init: handles both default export and plain module objects
function init(mod) {
  const fn = (mod && typeof mod === 'object') ? mod.default : mod;
  if (typeof fn === 'function') fn(Highcharts);
}

// Use the classic module paths (resolve reliably with v11 + Vite)
import * as HighchartsMore from 'highcharts/highcharts-more';
import * as Accessibility from 'highcharts/modules/accessibility';
import * as Treemap from 'highcharts/modules/treemap';
import * as SolidGauge from 'highcharts/modules/solid-gauge';

// Initialize (no packed-bubble)
init(HighchartsMore);
init(Accessibility);
init(Treemap);
init(SolidGauge);

export default Highcharts;