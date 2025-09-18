
function hashString(str) {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  return hash;
}

export function getArtistColour(artistName) {
  if (!artistName) return '#999';
  
  const hash = hashString(artistName.toLowerCase());
  const hue = Math.abs(hash) % 360;
  const saturation = 65;
  const lightness = 55;

  return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
}