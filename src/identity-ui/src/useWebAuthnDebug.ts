import * as CBOR from 'cbor';

export type CredentialOptions = {
  authenticator?: 'platform' | 'cross-platform' | undefined;
  attestation?: 'direct' | 'indirect' | 'none' | undefined;
  verification?: 'required' | 'discouraged' | 'preferred' | undefined;
  format?: 'base64url' | 'hex' | undefined;
};

const COSEKEYS: { [id: string]: number } = {
  kty: 1,
  alg: 3,
  crv: -1,
  x: -2,
  y: -3,
  n: -1,
  e: -2,
};

// const COSEKTY: { [id: string]: number } = {
//   OKP: 1,
//   EC2: 2,
//   RSA: 3,
// };

const COSEKTY: { [id: string]: string } = {
  '1': 'OKP',
  '2': 'EC2',
  '3': 'RSA',
};

const COSERSASCHEME: { [id: string]: string } = {
  '-3': 'pss-sha256',
  '-39': 'pss-sha512',
  '-38': 'pss-sha384',
  '-65535': 'pkcs1-sha1',
  '-257': 'pkcs1-sha256',
  '-258': 'pkcs1-sha384',
  '-259': 'pkcs1-sha512',
};

const COSECRV: { [id: string]: string } = {
  '1': 'p256',
  '2': 'p384',
  '3': 'p521',
};

const COSEALGHASH: { [id: string]: string } = {
  '-257': 'sha256', // 'RSASSA-PKCS1-v1_5 w/ SHA-256',
  '-258': 'sha384', // 'RSASSA-PKCS1-v1_5 w/ SHA-384',
  '-259': 'sha512', // 'RSASSA-PKCS1-v1_5 w/ SHA-512',
  '-65535': 'sha1', // 'RSASSA-PKCS1-v1_5 w/ SHA-1',
  '-39': 'sha512', // 'RSASSA-PSS w/ SHA-512',
  '-38': 'sha384', // 'RSASSA-PSS w/ SHA-384',
  '-37': 'sha256', // 'RSASSA-PSS w/ SHA-256',
  '-260': 'sha256',
  '-261': 'sha512',
  '-7': 'sha256', // 'ECDSA w/ SHA-256',
  '-35': 'sha384', // 'ECDSA w/ SHA-384',
  '-36': 'sha512', // 'ECDSA w/ SHA-512',
};

const COSEALGNAME: { [id: string]: string } = {
  '-65535': 'RSASSA-PKCS1-v1_5 w/ SHA-1',
  '-257': 'RSASSA-PKCS1-v1_5 w/ SHA-256',
  '-258': 'RSASSA-PKCS1-v1_5 w/ SHA-384',
  '-259': 'RSASSA-PKCS1-v1_5 w/ SHA-512',
  '-260': 'UNKNOWN w/ SHA-256',
  '-261': 'UNKNOWN w/ SHA-512',
  '-37': 'RSASSA-PSS w/ SHA-256',
  '-38': 'RSASSA-PSS w/ SHA-384',
  '-39': 'RSASSA-PSS w/ SHA-512',
  '-7': 'ECDSA w/ SHA-256',
  '-35': 'ECDSA w/ SHA-384',
  '-36': 'ECDSA w/ SHA-512',
  '-47': 'ECDSA-SECP256K1 w/ SHA-256',
  '-8': 'EDDSA',
};

export default function useWebAuthn() {
  const uuidv4 = () =>
    ('' + 1e7 + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, (c) =>
      ((c as any) ^ (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> ((c as any) / 4)))).toString(16)
    );

  const buf2hex = (buffer: ArrayBuffer) => arr2hex(new Uint8Array(buffer));

  const arr2hex = (arr: Uint8Array) => Array.prototype.map.call(arr, (b) => ('00' + b.toString(16)).slice(-2)).join('');

  const parseAuthData = (buffer: Uint8Array): any => {
    const rpIdHash = arr2hex(buffer.slice(0, 32));
    buffer = buffer.slice(32);

    const flags = buffer.slice(0, 1)[0];
    const userPresent = !!(flags & 0x01);
    const userVerified = !!(flags & 0x04);
    const attestedCredentialData = !!(flags && 0x40);
    const extensionData = !!(flags && 0x80);
    buffer = buffer.slice(1);

    const counter = Buffer.from(buffer.slice(0, 4)).readUInt32BE(0);
    buffer = buffer.slice(4);

    let acd = {};
    if (attestedCredentialData) {
      const aaguid = arr2hex(buffer.slice(0, 16));
      buffer = buffer.slice(16);

      const credentialIdLength = Buffer.from(buffer.slice(0, 2)).readUInt16BE(0);
      buffer = buffer.slice(2);

      const credentialId = arr2hex(buffer.slice(0, credentialIdLength));
      buffer = buffer.slice(credentialIdLength);

      const publicKey = CBOR.decode(buffer);
      console.log(publicKey);
      const hashAlgName = COSEALGNAME[publicKey.get(COSEKEYS.alg)];
      const keyType = COSEKTY[publicKey.get(COSEKEYS.kty)];
      let x: any = undefined;
      let y: any = undefined;
      let curve: any = undefined;
      switch (keyType) {
        case 'EC2':
          x = arr2hex(publicKey.get(COSEKEYS.x) as Uint8Array);
          y = arr2hex(publicKey.get(COSEKEYS.y) as Uint8Array);
          curve = COSECRV[publicKey.get(COSEKEYS.crv)];
          break;
        case 'RSA':
          break;
        case 'OKP':
          break;
      }
      acd = {
        aaguid,
        credentialIdLength,
        credentialId,
        publicKey: {
          hashAlgName,
          keyType,
          ...(curve && { curve }),
          ...(x && { x }),
          ...(y && { y }),
        },
      };
    }

    return {
      rpIdHash,
      userPresent,
      userVerified,
      attestedCredentialData,
      extensionData,
      counter,
      ...(attestedCredentialData && { attestedCredentialData: acd }),
    };
  };

  const getCreateCredentialObject = (
    username: string,
    relyingParty: PublicKeyCredentialRpEntity,
    serverChallenge: Uint8Array,
    options: CredentialOptions,
    excludeCredentials: string[]
  ): PublicKeyCredentialCreationOptions => {
    return {
      rp: relyingParty,
      user: {
        name: username,
        displayName: username,
        id: Uint8Array.from(uuidv4(), (c) => c.charCodeAt(0)),
      },
      challenge: serverChallenge,
      pubKeyCredParams: [
        { type: 'public-key', alg: -7 },
        { type: 'public-key', alg: -35 },
        { type: 'public-key', alg: -36 },
        { type: 'public-key', alg: -257 },
        { type: 'public-key', alg: -258 },
        { type: 'public-key', alg: -259 },
        { type: 'public-key', alg: -37 },
        { type: 'public-key', alg: -38 },
        { type: 'public-key', alg: -39 },
        { type: 'public-key', alg: -8 },
      ],
      ...(options.attestation && { attestation: options.attestation }),
      authenticatorSelection: {
        requireResidentKey: false,
        ...(options.verification && { userVerification: options.verification }),
        ...(options.authenticator && { authenticatorAttachment: options.authenticator }),
      },
      excludeCredentials: excludeCredentials.map((cred) => {
        return {
          id: Uint8Array.from(cred, (c) => c.charCodeAt(0)),
          type: 'public-key',
        };
      }),
      extensions: {
        txAuthSimple: 'CCCCCCRRRREEEEAAATTTEEE',
      },
    };
  };

  const getRequestCredentialObject = (
    serverChallenge: Uint8Array,
    allowCredentials: string[],
    verification: boolean,
    relyingPartyId?: string
  ): PublicKeyCredentialRequestOptions => {
    return {
      challenge: serverChallenge,
      allowCredentials: allowCredentials.map((cred) => {
        return {
          id: Uint8Array.from(atob(cred), (c) => c.charCodeAt(0)),
          type: 'public-key',
        };
      }),
      userVerification: verification ? 'required' : 'discouraged',
      rpId: relyingPartyId,
      extensions: {
        txAuthSimple: 'RRRREEEEQUUUEEESSSSSTTTT',
      },
    };
  };

  const createCredentials = async (
    credentialName: string,
    relyingParty: PublicKeyCredentialRpEntity,
    serverChallenge: Uint8Array,
    options: CredentialOptions,
    excludeCredentials: string[] = []
  ): Promise<PublicKeyCredential> => {
    console.log('server challenge: ' + arr2hex(serverChallenge));
    const request = getCreateCredentialObject(
      credentialName,
      relyingParty,
      serverChallenge,
      options,
      excludeCredentials
    );
    const credentials = (await navigator.credentials.create({
      publicKey: request,
    })) as PublicKeyCredential;

    console.info(credentials);
    const clientData = JSON.parse(
      String.fromCharCode(...Array.from(new Uint8Array(credentials.response.clientDataJSON)))
    );
    console.info(clientData);
    const attestationObject = CBOR.decode((credentials.response as AuthenticatorAttestationResponse).attestationObject);
    console.log(attestationObject);
    const authData = parseAuthData(attestationObject.authData);
    console.log(authData);
    const cred = credentialName + '|' + btoa(String.fromCharCode(...Array.from(new Uint8Array(credentials.rawId))));
    console.info(`registered credentials ${cred}`);
    return credentials;
  };

  const requestCredentials = async (
    serverChallenge: Uint8Array,
    allowCredentials: string[] = [],
    verification: boolean = false,
    relyingPartyId?: string
  ): Promise<PublicKeyCredential> => {
    const request = getRequestCredentialObject(serverChallenge, allowCredentials, verification, relyingPartyId);
    const credentials = (await navigator.credentials.get({ publicKey: request })) as PublicKeyCredential;
    console.info(credentials);
    return credentials;
  };

  return {
    createCredentials,
    requestCredentials,
  };
}
