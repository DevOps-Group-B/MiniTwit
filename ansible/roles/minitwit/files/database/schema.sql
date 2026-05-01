
-- Dumped from database version 16.13 (Ubuntu 16.13-0ubuntu0.24.04.1)
-- Dumped by pg_dump version 16.13 (Ubuntu 16.13-0ubuntu0.24.04.1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Achievements; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."Achievements" (
    "AchievementId" bigint NOT NULL,
    "Title" text,
    "Description" text,
    "ImagePath" text
);


ALTER TABLE public."Achievements" OWNER TO minitwit_user;

--
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetRoleClaims" (
    id bigint NOT NULL,
    roleid text,
    claimtype text,
    claimvalue text
);


ALTER TABLE public."AspNetRoleClaims" OWNER TO minitwit_user;

--
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetRoles" (
    id text,
    name text,
    normalizedname text,
    concurrencystamp text
);


ALTER TABLE public."AspNetRoles" OWNER TO minitwit_user;

--
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetUserClaims" (
    id bigint NOT NULL,
    userid text,
    claimtype text,
    claimvalue text
);


ALTER TABLE public."AspNetUserClaims" OWNER TO minitwit_user;

--
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetUserLogins" (
    loginprovider text,
    providerkey text,
    providerdisplayname text,
    userid text
);


ALTER TABLE public."AspNetUserLogins" OWNER TO minitwit_user;

--
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetUserRoles" (
    userid text,
    roleid text
);


ALTER TABLE public."AspNetUserRoles" OWNER TO minitwit_user;

--
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetUserTokens" (
    userid text,
    loginprovider text,
    name text,
    value text
);


ALTER TABLE public."AspNetUserTokens" OWNER TO minitwit_user;

--
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AspNetUsers" (
    "Id" text,
    "Name" text,
    "Bio" text,
    "UserName" text,
    "NormalizedUserName" text,
    "Email" text,
    "NormalizedEmail" text,
    "EmailConfirmed" bigint,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" bigint,
    "TwoFactorEnabled" bigint,
    "LockoutEnd" text,
    "LockoutEnabled" bigint,
    "AccessFailedCount" bigint
);


ALTER TABLE public."AspNetUsers" OWNER TO minitwit_user;

--
-- Name: AuthorAchievements; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AuthorAchievements" (
    "AuthorId" text,
    "AchievementId" bigint,
    "AchievedAt" timestamp with time zone
);


ALTER TABLE public."AuthorAchievements" OWNER TO minitwit_user;

--
-- Name: AuthorFollowers; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."AuthorFollowers" (
    "FollowerId" text,
    "FollowingId" text
);


ALTER TABLE public."AuthorFollowers" OWNER TO minitwit_user;

--
-- Name: Cheeps; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."Cheeps" (
    "CheepId" bigint NOT NULL,
    "Text" text,
    "TimeStamp" timestamp with time zone,
    "AuthorId" text
);


ALTER TABLE public."Cheeps" OWNER TO minitwit_user;

--
-- Name: Latest; Type: TABLE; Schema: public; Owner: minitwit_user
--

CREATE TABLE public."Latest" (
    "Id" bigint NOT NULL,
    "Value" bigint
);


ALTER TABLE public."Latest" OWNER TO minitwit_user;

--
-- Name: achievements_achievementid_seq; Type: SEQUENCE; Schema: public; Owner: minitwit_user
--

CREATE SEQUENCE public.achievements_achievementid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.achievements_achievementid_seq OWNER TO minitwit_user;

--
-- Name: achievements_achievementid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: minitwit_user
--

ALTER SEQUENCE public.achievements_achievementid_seq OWNED BY public."Achievements"."AchievementId";


--
-- Name: cheeps_cheepid_seq; Type: SEQUENCE; Schema: public; Owner: minitwit_user
--

CREATE SEQUENCE public.cheeps_cheepid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cheeps_cheepid_seq OWNER TO minitwit_user;

--
-- Name: cheeps_cheepid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: minitwit_user
--

ALTER SEQUENCE public.cheeps_cheepid_seq OWNED BY public."Cheeps"."CheepId";


--
-- Name: latest_id_seq; Type: SEQUENCE; Schema: public; Owner: minitwit_user
--

CREATE SEQUENCE public.latest_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.latest_id_seq OWNER TO minitwit_user;

--
-- Name: latest_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: minitwit_user
--

ALTER SEQUENCE public.latest_id_seq OWNED BY public."Latest"."Id";


--
-- Name: Achievements AchievementId; Type: DEFAULT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Achievements" ALTER COLUMN "AchievementId" SET DEFAULT nextval('public.achievements_achievementid_seq'::regclass);


--
-- Name: Cheeps CheepId; Type: DEFAULT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Cheeps" ALTER COLUMN "CheepId" SET DEFAULT nextval('public.cheeps_cheepid_seq'::regclass);


--
-- Name: Latest Id; Type: DEFAULT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Latest" ALTER COLUMN "Id" SET DEFAULT nextval('public.latest_id_seq'::regclass);


--
-- Name: Achievements idx_16391_achievements_pkey; Type: CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Achievements"
    ADD CONSTRAINT idx_16391_achievements_pkey PRIMARY KEY ("AchievementId");


--
-- Name: Latest idx_16408_latest_pkey; Type: CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Latest"
    ADD CONSTRAINT idx_16408_latest_pkey PRIMARY KEY ("Id");


--
-- Name: AspNetRoleClaims idx_16412_aspnetroleclaims_pkey; Type: CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT idx_16412_aspnetroleclaims_pkey PRIMARY KEY (id);


--
-- Name: AspNetUserClaims idx_16417_aspnetuserclaims_pkey; Type: CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT idx_16417_aspnetuserclaims_pkey PRIMARY KEY (id);


--
-- Name: Cheeps idx_16449_cheeps_pkey; Type: CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Cheeps"
    ADD CONSTRAINT idx_16449_cheeps_pkey PRIMARY KEY ("CheepId");


--
-- Name: idx_16397_rolenameindex; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16397_rolenameindex ON public."AspNetRoles" USING btree (normalizedname);


--
-- Name: idx_16397_sqlite_autoindex_aspnetroles_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16397_sqlite_autoindex_aspnetroles_1 ON public."AspNetRoles" USING btree (id);


--
-- Name: idx_16402_emailindex; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16402_emailindex ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- Name: idx_16402_ix_aspnetusers_name; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16402_ix_aspnetusers_name ON public."AspNetUsers" USING btree ("Name");


--
-- Name: idx_16402_sqlite_autoindex_aspnetusers_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16402_sqlite_autoindex_aspnetusers_1 ON public."AspNetUsers" USING btree ("Id");


--
-- Name: idx_16402_usernameindex; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16402_usernameindex ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- Name: idx_16412_ix_aspnetroleclaims_roleid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16412_ix_aspnetroleclaims_roleid ON public."AspNetRoleClaims" USING btree (roleid);


--
-- Name: idx_16417_ix_aspnetuserclaims_userid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16417_ix_aspnetuserclaims_userid ON public."AspNetUserClaims" USING btree (userid);


--
-- Name: idx_16422_ix_aspnetuserlogins_userid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16422_ix_aspnetuserlogins_userid ON public."AspNetUserLogins" USING btree (userid);


--
-- Name: idx_16422_sqlite_autoindex_aspnetuserlogins_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16422_sqlite_autoindex_aspnetuserlogins_1 ON public."AspNetUserLogins" USING btree (loginprovider, providerkey);


--
-- Name: idx_16427_ix_aspnetuserroles_roleid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16427_ix_aspnetuserroles_roleid ON public."AspNetUserRoles" USING btree (roleid);


--
-- Name: idx_16427_sqlite_autoindex_aspnetuserroles_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16427_sqlite_autoindex_aspnetuserroles_1 ON public."AspNetUserRoles" USING btree (userid, roleid);


--
-- Name: idx_16432_sqlite_autoindex_aspnetusertokens_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16432_sqlite_autoindex_aspnetusertokens_1 ON public."AspNetUserTokens" USING btree (userid, loginprovider, name);


--
-- Name: idx_16437_ix_authorachievements_achievementid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16437_ix_authorachievements_achievementid ON public."AuthorAchievements" USING btree ("AchievementId");


--
-- Name: idx_16437_sqlite_autoindex_authorachievements_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16437_sqlite_autoindex_authorachievements_1 ON public."AuthorAchievements" USING btree ("AuthorId", "AchievementId");


--
-- Name: idx_16443_ix_authorfollowers_followingid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16443_ix_authorfollowers_followingid ON public."AuthorFollowers" USING btree ("FollowingId");


--
-- Name: idx_16443_sqlite_autoindex_authorfollowers_1; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE UNIQUE INDEX idx_16443_sqlite_autoindex_authorfollowers_1 ON public."AuthorFollowers" USING btree ("FollowerId", "FollowingId");


--
-- Name: idx_16449_ix_cheeps_authorid; Type: INDEX; Schema: public; Owner: minitwit_user
--

CREATE INDEX idx_16449_ix_cheeps_authorid ON public."Cheeps" USING btree ("AuthorId");


--
-- Name: AspNetRoleClaims aspnetroleclaims_roleid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT aspnetroleclaims_roleid_fkey FOREIGN KEY (roleid) REFERENCES public."AspNetRoles"(id) ON DELETE CASCADE;


--
-- Name: AspNetUserClaims aspnetuserclaims_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT aspnetuserclaims_userid_fkey FOREIGN KEY (userid) REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserLogins aspnetuserlogins_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT aspnetuserlogins_userid_fkey FOREIGN KEY (userid) REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles aspnetuserroles_roleid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT aspnetuserroles_roleid_fkey FOREIGN KEY (roleid) REFERENCES public."AspNetRoles"(id) ON DELETE CASCADE;


--
-- Name: AspNetUserRoles aspnetuserroles_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT aspnetuserroles_userid_fkey FOREIGN KEY (userid) REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserTokens aspnetusertokens_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT aspnetusertokens_userid_fkey FOREIGN KEY (userid) REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AuthorAchievements authorachievements_achievementid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AuthorAchievements"
    ADD CONSTRAINT authorachievements_achievementid_fkey FOREIGN KEY ("AchievementId") REFERENCES public."Achievements"("AchievementId") ON DELETE CASCADE;


--
-- Name: AuthorAchievements authorachievements_authorid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AuthorAchievements"
    ADD CONSTRAINT authorachievements_authorid_fkey FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AuthorFollowers authorfollowers_followerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AuthorFollowers"
    ADD CONSTRAINT authorfollowers_followerid_fkey FOREIGN KEY ("FollowerId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AuthorFollowers authorfollowers_followingid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."AuthorFollowers"
    ADD CONSTRAINT authorfollowers_followingid_fkey FOREIGN KEY ("FollowingId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: Cheeps cheeps_authorid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: minitwit_user
--

ALTER TABLE ONLY public."Cheeps"
    ADD CONSTRAINT cheeps_authorid_fkey FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict lp8NuybcyUdXhcGLtWg7YXwXJ1gDI9Qg4KEV4lBc9dcZ0mQ5wOykWfhQFTXT62q
