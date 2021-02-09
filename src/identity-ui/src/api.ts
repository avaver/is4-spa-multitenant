export const api = async (url: string, method: string = 'GET', body?: any) => {
  console.debug(`${method} ${url}`);
  const response = await fetch(url, {
    method,
    ...((method === 'POST' || method === 'PATCH') && {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }),
  });
  if (response.redirected) {
    console.debug(`REDIRECT ${response.url}`);
    window.location.href = response.url;
    return;
  }
  console.debug(`${response.status} ${response.statusText}`);
  if (response.status === 200) {
    try {
      const data = await response.json();
      console.debug(data);
      return data;
    } catch {
      return {};
    }
  } else {
    return { error: response.statusText };
  }
};
