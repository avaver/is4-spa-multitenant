import { b64url2arr, str2arr } from './utils';

const PKCT = 'public-key';

export type CredentialUserInfo = {
  id: string;
  name: string;
  displayName: string;
};

export type CredentialOptions = {
  authenticator?: AuthenticatorAttachment;
  attestation?: AttestationConveyancePreference;
  verification?: UserVerificationRequirement;
  residentKey?: boolean;
};

export const attestationRequest = (
  user: CredentialUserInfo,
  rp: PublicKeyCredentialRpEntity,
  serverChallenge: string,
  attestation?: AttestationConveyancePreference,
  authenticatorSelection?: AuthenticatorSelectionCriteria,
  excludeCredentials?: string[],
  timeout?: number
): PublicKeyCredentialCreationOptions => ({
  rp,
  user: {
    id: str2arr(user.id),
    name: user.name,
    displayName: user.displayName,
  },
  challenge: b64url2arr(serverChallenge),
  pubKeyCredParams: [
    { type: PKCT, alg: -7 },
    { type: PKCT, alg: -35 },
    { type: PKCT, alg: -36 },
    { type: PKCT, alg: -257 },
    { type: PKCT, alg: -258 },
    { type: PKCT, alg: -259 },
    { type: PKCT, alg: -37 },
    { type: PKCT, alg: -38 },
    { type: PKCT, alg: -39 },
    { type: PKCT, alg: -8 },
  ],
  ...(attestation && { attestation }),
  ...(authenticatorSelection && { authenticatorSelection }),
  ...(excludeCredentials && { excludeCredentials: creds2list(excludeCredentials) }),
  ...(timeout && { timeout }),
  extensions: {
    uvm: true,
  },
});

export const assertionRequest = (
  serverChallenge: string,
  userVerification?: UserVerificationRequirement,
  rpId?: string,
  allowCredentials?: string[],
  timeout?: number
): PublicKeyCredentialRequestOptions => ({
  challenge: b64url2arr(serverChallenge),
  allowCredentials: allowCredentials?.map((cred) => {
    return {
      id: b64url2arr(cred),
      type: PKCT,
    };
  }),
  ...(userVerification && { userVerification }),
  ...(rpId && { rpId }),
  ...(timeout && { timeout }),
  extensions: {
    uvm: true,
  },
});

const creds2list = (credentials: string[]): PublicKeyCredentialDescriptor[] =>
  credentials.map((cred) => ({ id: b64url2arr(cred), type: PKCT }));
