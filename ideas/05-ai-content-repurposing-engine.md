# Idea 5: RepurposeAI — AI Content Repurposing Engine

## What Problem It Solves

- **Who has this problem?** Content creators, solopreneurs, and small business owners who create long-form content (podcasts, YouTube videos, blog posts, webinars) but lack time to repurpose it across platforms.
- **Why it's painful or urgent:** A single podcast episode could become 10 tweets, 3 LinkedIn posts, a newsletter, a blog post, and 5 short-form video scripts. But doing that manually takes 3-5 hours per piece. Most creators just... don't, leaving massive distribution value on the table.
- **Why people already pay for solutions:** Repurpose.io ($25-$79/mo), Opus Clip ($19-$49/mo), and Castmagic ($23-$99/mo) all have paying users. Freelance content repurposers charge $500-$2,000/mo per client. The demand is proven and growing.

## One-Person Advantage

This is a system-building business, not a service business. You build the repurposing pipeline once (AI prompts + templates + automation), then sell access to it. No employees needed — the AI does the heavy lifting, you maintain the quality of the system. A solo operator with strong content instincts can tune the AI output better than a team building generic one-size-fits-all tooling.

## Core Offer

**AI-powered service + Template system**

- User uploads or pastes their long-form content (transcript, blog post, video URL)
- System outputs a complete "content package": 10 tweets/X posts, 3 LinkedIn posts, 1 newsletter draft, 1 blog summary, 5 short-form video scripts with hooks
- All outputs match the creator's tone and style (set during onboarding)
- Delivered in a clean dashboard or as downloadable files (Notion, Google Docs, CSV)

## How It Makes Money

| Model | Price | Type |
|-------|-------|------|
| Pay-per-use (1 content piece) | $9-$15 | One-time |
| Pro plan (10 pieces/mo) | $39/mo | Recurring |
| Unlimited plan | $79/mo | Recurring |
| White-label for agencies | $199/mo | Recurring |

**At 100 subscribers (blended):** ~$4,500-$6,000/mo MRR.

## Distribution Strategy (Solo-Friendly)

- **X (Twitter):** Show real examples — "I turned a 45-minute podcast into 30 pieces of content in 2 minutes." Visual before/after threads perform extremely well.
- **YouTube:** Tutorial-style: "How I repurpose one video into 30 posts using AI." Targets creators already thinking about this problem.
- **Product Hunt:** Perfect launch platform — AI tools consistently trend on PH.
- **Podcaster/creator communities:** Guest on podcasts about content marketing. The audience IS the customer.
- **Content style:** Transformation demos. Show the input (one podcast) and the output (30 pieces). Let the result sell itself.

## AI & Automation Leverage

- AI is the entire product. LLM APIs (Claude/GPT) handle transcription-to-content transformation.
- Chain prompts for different platforms: tweet-style output uses different prompts than LinkedIn-style.
- Use Whisper API for automatic transcription from audio/video uploads.
- Automate delivery: Zapier/Make pushes outputs to user's Notion, Google Drive, or email.
- AI learns user's tone from their first 3 pieces — improves personalization over time.

## Setup Complexity

**Medium**

Needs a web app with: file upload, API calls to AI services (Claude/OpenAI), output formatting, and user accounts. Can be built with Next.js + Supabase + Stripe in a weekend if you scope tightly. The hard part is prompt engineering for high-quality, platform-specific outputs — but that's your competitive moat.

## Long-Term Scalability

- API costs scale linearly and decrease over time as AI gets cheaper.
- Add new output formats without rebuilding (TikTok scripts, Instagram captions, email sequences) — each is just a new prompt template.
- Build integrations with podcast hosts (Riverside, Descript) and CMS platforms for automatic repurposing on publish.
- White-label to marketing agencies who resell to their clients.
- Content library of "best repurposing prompts" becomes a sellable asset on its own.

---

## Weekend MVP Action Plan

1. **Saturday morning:** Build a single-page web app (Next.js + Vercel). One input field: "paste your transcript or blog post." Wire up Claude/OpenAI API to process the input.
2. **Saturday afternoon:** Write and test 5 prompt templates: tweet thread (10 posts), LinkedIn post (3 versions), newsletter draft, blog summary, short-form video script (5 hooks). Iterate until outputs are genuinely good.
3. **Sunday morning:** Add Stripe Checkout ($9 per use, $39/mo subscription). Build a simple output page that displays all generated content with copy-to-clipboard buttons.
4. **Sunday afternoon:** Process your own content (or a friend's podcast) through the tool. Screenshot the results. Write a Twitter thread and Product Hunt "upcoming" listing.
5. **Ship it.** First paying user who saves 3 hours of repurposing work validates the entire model.
