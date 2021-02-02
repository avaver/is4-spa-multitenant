/* eslint-disable prettier/prettier */
const sanitizeb64 = (b64: string) => b64.replace(/\//g, '_').replace(/\+/g, '-').replace(/=/g, '');
const desanitizeb64 = (b64url: string) => b64url.replace(/_/g, '/').replace(/-/g, '+');
export const arr2hex = (arr: Uint8Array) => Array.prototype.map.call(arr, (b) => ('00' + b.toString(16)).slice(-2)).join('');
export const buf2hex = (buf: ArrayBuffer) => arr2hex(new Uint8Array(buf));
export const buf2str = (buf: ArrayBuffer) => String.fromCharCode(...Array.from(new Uint8Array(buf)));
export const arr2b64url = (arr: Uint8Array) => sanitizeb64(btoa(String.fromCharCode(...Array.from(arr))));
export const buf2b64url = (buf: ArrayBuffer) => arr2b64url(new Uint8Array(buf));
export const b64url2arr = (b64url: string) => Uint8Array.from(atob(desanitizeb64(b64url)), (c) => c.charCodeAt(0));
export const str2arr = (str: string) => Uint8Array.from(str, (c) => c.charCodeAt(0));
export const uuidv4 = () => ('' + 1e7 + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, (c) => ((c as any) ^ (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> ((c as any) / 4)))).toString(16));

/* eslint-enable prettier/prettier */

export const getPrintableCopy = (obj: any) => {
  if (obj instanceof ArrayBuffer) {
    return buf2b64url(obj);
  }
  if (obj instanceof Uint8Array) {
    return arr2b64url(obj);
  }

  const copy: any = {};
  for (const key in obj) {
    const value = obj[key];
    if (typeof value === 'function') {
      continue;
    } else if (value instanceof Uint8Array) {
      copy[key] = arr2b64url(value);
    } else if (value instanceof ArrayBuffer) {
      copy[key] = buf2b64url(value);
    } else if (value instanceof Object) {
      copy[key] = getPrintableCopy(value);
    } else {
      copy[key] = value;
    }
  }

  return copy;
};
